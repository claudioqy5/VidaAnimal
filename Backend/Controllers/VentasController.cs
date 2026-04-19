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
                    // Manejo de zona horaria Perú multiplataforma
                    TimeZoneInfo peruTimeZone;
                    try {
                        peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                    } catch {
                        try {
                            peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");
                        } catch {
                            peruTimeZone = TimeZoneInfo.Local;
                        }
                    }

                    // Calculamos el inicio y fin del día en Perú, pero convertido a UTC para comparar en la DB
                    // Si el usuario filtra por "2024-04-05", queremos todo lo que sea >= 00:00 (Perú) y < 00:00 del día siguiente (Perú)
                    var inicioDiaPeru = fechaFiltro.Date;
                    var finDiaPeru = inicioDiaPeru.AddDays(1);

                    // La BD ya guarda la hora en zona horaria local de Perú, no necesitamos sumar las 5 horas de UTC
                    query = query.Where(v => v.Fecha >= inicioDiaPeru && v.Fecha < finDiaPeru);
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
                v.MetodoPago,
                v.Estado,
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
                    MetodoPago = string.IsNullOrEmpty(req.MetodoPago) ? "Efectivo" : req.MetodoPago,
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

                    // Lógica especial de stock para Sacos vendidos por Kilo y Baldes por Unidad
                    decimal decrementoStockActual = det.Cantidad;
                    decimal costoUnitarioAjustado = p.PrecioCosto; // Costo por bulto por defecto

                    if ((p.UnidadMedida == "SACO" && det.UnidadVenta == "KG") || 
                        (p.UnidadMedida == "BALDE" && det.UnidadVenta == "UND")) 
                    {
                        if (p.CantidadMayorista.HasValue && p.CantidadMayorista > 0)
                        {
                            decrementoStockActual = det.Cantidad / p.CantidadMayorista.Value;
                            // El costo de 1 KG es el (Costo del Saco / Peso del Saco)
                            costoUnitarioAjustado = p.PrecioCosto / p.CantidadMayorista.Value;
                        }
                    }

                    if (p.StockActual < decrementoStockActual)
                        return BadRequest(new { success = false, mensaje = $"Stock insuficiente para '{p.Nombre}'. Solicitado: {decrementoStockActual} sacos, Disponible: {p.StockActual} sacos." });

                    itemsVenta.Add((p, decrementoStockActual, det.PrecioVentaUnitario, p.StockActual));

                    // Calcular la ganancia de esta línea de producto
                    decimal gananciaLinea = (det.PrecioVentaUnitario - costoUnitarioAjustado) * det.Cantidad;

                    // Descontar stock
                    p.StockActual -= decrementoStockActual;

                    // Agregar detalle a la venta con el margen capturado
                    nuevaVenta.VentaDetalles.Add(new VentaDetalle
                    {
                        ProductoID = det.ProductoID,
                        Cantidad = det.Cantidad, 
                        PrecioVentaUnitario = det.PrecioVentaUnitario,
                        PrecioCostoUnitario = costoUnitarioAjustado, // CAPTURAMOS EL COSTO
                        Ganancia = gananciaLinea, // CAPTURAMOS EL MARGEN REAL
                        UnidadVenta = det.UnidadVenta ?? p.UnidadMedida 
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

        public class AnularRequest 
        {
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("{id}/anular")]
        public async Task<IActionResult> AnularVenta(int id, [FromBody] AnularRequest req)
        {
            var venta = await _context.Ventas
                .Include(v => v.VentaDetalles)
                .FirstOrDefaultAsync(v => v.VentaID == id);

            if (venta == null) return NotFound(new { success = false, mensaje = "Venta no encontrada." });
            if (venta.Estado == "Anulada") return BadRequest(new { success = false, mensaje = "La venta ya está anulada." });

            var claimUsuarioId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            int? usuarioId = null;
            if (int.TryParse(claimUsuarioId, out int uid)) usuarioId = uid;

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(req.Password, usuario.PasswordHash))
            {
                return BadRequest(new { success = false, mensaje = "Contraseña de administrador incorrecta. Operación denegada." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                venta.Estado = "Anulada";

                foreach (var det in venta.VentaDetalles)
                {
                    var p = await _context.Productos.FindAsync(det.ProductoID);
                    if (p != null)
                    {
                        decimal incrementoStockActual = det.Cantidad;
                        if ((p.UnidadMedida == "SACO" && det.UnidadVenta == "KG") || 
                            (p.UnidadMedida == "BALDE" && det.UnidadVenta == "UND")) 
                        {
                            if (p.CantidadMayorista.HasValue && p.CantidadMayorista > 0)
                            {
                                incrementoStockActual = det.Cantidad / p.CantidadMayorista.Value;
                            }
                        }

                        var stockAnterior = p.StockActual;
                        p.StockActual += incrementoStockActual;

                        var kardex = new MovimientoInventario
                        {
                            Fecha = DateTime.Now,
                            Tipo = "DEVOLUCION",
                            ProductoID = p.ProductoID,
                            Cantidad = incrementoStockActual,
                            UsuarioID = usuarioId ?? 1,
                            StockAnterior = stockAnterior,
                            StockNuevo = stockAnterior + incrementoStockActual,
                            ReferenciaID = venta.VentaID,
                            Observaciones = $"Anulación de Venta #{venta.VentaID} - {venta.SerieComprobante}-{venta.NumeroComprobante}",
                        };
                        _context.MovimientosInventario.Add(kardex);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, mensaje = "Venta anulada y stock devuelto exitosamente." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innermost = ex;
                while (innermost.InnerException != null) innermost = innermost.InnerException;
                return StatusCode(500, new { success = false, mensaje = "Error al anular venta: " + innermost.Message });
            }
        }
    }

    public class VentaRequestDTO
    {
        public int ClienteID { get; set; }
        public string SerieComprobante { get; set; } = string.Empty;
        public string NumeroComprobante { get; set; } = string.Empty;
        public decimal Descuento { get; set; } = 0;
        public string? MetodoPago { get; set; }
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
