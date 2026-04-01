using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetVentas([FromQuery] int? clienteId, [FromQuery] string? fecha)
        {
            var query = _context.Ventas
                .Include(v => v.VentaDetalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.Fecha)
                .AsQueryable();

            if (clienteId.HasValue && clienteId.Value > 0)
                query = query.Where(v => v.ClienteID == clienteId.Value);

            if (!string.IsNullOrEmpty(fecha))
            {
                if (DateTime.TryParse(fecha, out DateTime fechaFiltro))
                {
                    query = query.Where(v => v.Fecha.Date == fechaFiltro.Date);
                }
            }

            var ventas = await query.Select(v => new {
                v.VentaID,
                v.Fecha,
                v.SubTotal,
                v.Total,
                v.Descuento,
                v.SerieComprobante,
                v.NumeroComprobante,
                v.Observaciones,
                Cajero = v.Usuario == null ? "SISTEMA" : v.Usuario.NombreCompleto,
                Cliente = v.Cliente == null ? null : new {
                    v.Cliente.ClienteID,
                    v.Cliente.NombreCompleto,
                    v.Cliente.DocumentoIdentidad,
                    v.Cliente.Telefono
                },
                DetalleVentas = v.VentaDetalles.Select(d => new {
                    d.VentaDetalleID,
                    d.ProductoID,
                    d.Cantidad,
                    PrecioUnitario = d.PrecioVentaUnitario,
                    d.SubTotal,
                    Producto = d.Producto == null ? null : new {
                        d.Producto.Nombre,
                        d.Producto.Codigo
                    }
                }).ToList()
            }).ToListAsync();

            return Ok(new { success = true, data = ventas });
        }


        [HttpPost]

        public async Task<IActionResult> RegistrarVenta([FromBody] VentaRequestDTO req)
        {
            try
            {
                var claimUsuarioId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                int? usuarioId = null;
                if (int.TryParse(claimUsuarioId, out int uid)) usuarioId = uid;

                var nuevaVenta = new Venta
                {
                    ClienteID = req.ClienteID == 0 ? null : req.ClienteID,
                    UsuarioID = usuarioId ?? 1,
                    SerieComprobante = req.SerieComprobante,
                    NumeroComprobante = req.NumeroComprobante,
                    Fecha = DateTime.Now,
                    Descuento = req.Descuento,
                    Observaciones = req.Observaciones,
                    Total = 0 
                };

                decimal totalCalculado = 0;

                // Paso 1: Validar stock y preparar datos (sin guardar aún)
                var itemsVenta = new List<(Producto producto, decimal cantidad, decimal precio, decimal stockAnterior)>();

                foreach (var det in req.Detalles)
                {
                    var p = await _context.Productos.FindAsync(det.ProductoID);
                    if (p == null) return BadRequest(new { success = false, mensaje = $"Producto con ID {det.ProductoID} no existe." });

                    // Lógica especial de stock para Sacos vendidos por Kilo
                    decimal decrementoStockActual = det.Cantidad;
                    if (p.UnidadMedida == "SACO" && det.UnidadVenta == "KG") 
                    {
                        // Si se vende por kilo, restamos (cantidad / peso_del_saco) del stock actual de sacos
                        if (p.CantidadMayorista.HasValue && p.CantidadMayorista > 0)
                        {
                            decrementoStockActual = det.Cantidad / p.CantidadMayorista.Value;
                        }
                    }

                    if (p.StockActual < decrementoStockActual)
                        return BadRequest(new { success = false, mensaje = $"Stock insuficiente para '{p.Nombre}'. Solicitado: {decrementoStockActual} sacos, Disponible: {p.StockActual} sacos." });

                    itemsVenta.Add((p, decrementoStockActual, det.PrecioVentaUnitario, p.StockActual));

                    // Descontar stock (decrementoStockActual ya está en unidades del stock)
                    p.StockActual -= decrementoStockActual;

                    // Agregar detalle a la venta
                    nuevaVenta.VentaDetalles.Add(new VentaDetalle
                    {
                        ProductoID = det.ProductoID,
                        Cantidad = det.Cantidad, // Se guarda la cantidad nominal vendida (ej: 2)
                        PrecioVentaUnitario = det.PrecioVentaUnitario,
                        UnidadVenta = det.UnidadVenta ?? p.UnidadMedida // Guardamos si fue KG o SACO
                    });

                    totalCalculado += (det.Cantidad * det.PrecioVentaUnitario);
                }

                nuevaVenta.SubTotal = totalCalculado;
                nuevaVenta.Total = totalCalculado - req.Descuento;

                // Paso 2: Guardar la Venta (genera el VentaID)
                _context.Ventas.Add(nuevaVenta);
                await _context.SaveChangesAsync();

                // Paso 3: Ahora que tenemos el VentaID, crear los registros de Kardex
                foreach (var (prod, cantidad, precio, stockAnterior) in itemsVenta)
                {
                    var kardex = new MovimientoInventario
                    {
                        Fecha = DateTime.Now,
                        Tipo = "SALIDA",
                        ProductoID = prod.ProductoID,
                        Cantidad = cantidad,
                        UsuarioID = nuevaVenta.UsuarioID,
                        StockAnterior = stockAnterior,
                        StockNuevo = stockAnterior - cantidad,
                        ReferenciaID = nuevaVenta.VentaID, // ✅ Ahora sí existe el ID
                        Observaciones = $"Venta #{nuevaVenta.VentaID} - {nuevaVenta.SerieComprobante}-{nuevaVenta.NumeroComprobante}",
                    };
                    _context.MovimientosInventario.Add(kardex);
                }

                await _context.SaveChangesAsync(); // Guardar Kardex

                return Ok(new { success = true, mensaje = "Venta exitosa y stock actualizado.", id = nuevaVenta.VentaID });
            }
            catch (Exception ex)
            {
                var innermost = ex;
                while (innermost.InnerException != null) innermost = innermost.InnerException;
                return BadRequest(new { success = false, mensaje = "Error interno al vender: " + innermost.Message });
            }
        }
    }

    public class VentaRequestDTO
    {
        public int ClienteID { get; set; }
        public string SerieComprobante { get; set; } = string.Empty;
        public string NumeroComprobante { get; set; } = string.Empty;
        public decimal Descuento { get; set; } = 0;
        public string? Observaciones { get; set; }
        public List<VentaDetalleDTO> Detalles { get; set; } = new List<VentaDetalleDTO>();
    }

    public class VentaDetalleDTO
    {
        public int ProductoID { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioVentaUnitario { get; set; }
        public string? UnidadVenta { get; set; }
    }
}
