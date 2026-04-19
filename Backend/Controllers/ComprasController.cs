using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Claims;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;
using VidaAnimal.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "ADMINISTRADOR")]
    public class ComprasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComprasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompras()
        {
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(c => c.FechaCompra)
                .ToListAsync();
            return Ok(new { success = true, data = compras });
        }

        [HttpPost]
        public async Task<IActionResult> CrearCompra([FromBody] CompraCreateDTO modelo)
        {
            if (modelo.Detalles == null || !modelo.Detalles.Any())
                return BadRequest(new { success = false, mensaje = "La compra debe tener al menos un producto." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Extraer el ID del usuario del Token JWT autenticado
                var claimUsuarioId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                int? parsedUsuarioId = int.TryParse(claimUsuarioId, out int uid) ? uid : (int?)null;

                // 1. Crear el encabezado de la Compra
                var nuevaCompra = new Compra
                {
                    ProveedorID = modelo.ProveedorID,
                    NumeroComprobante = modelo.NumeroComprobante,
                    FechaCompra = DateTime.UtcNow,
                    Total = modelo.Total,
                    UsuarioID = parsedUsuarioId // ID del que hizo login en el navegador
                };

                _context.Compras.Add(nuevaCompra);
                await _context.SaveChangesAsync();

                // 2. Procesar Detalles, Actualizar Stock y registrar en Kardex
                var itemsCompra = new List<(Producto producto, decimal cantidad, decimal stockAnterior)>();

                foreach (var det in modelo.Detalles)
                {
                    var productoDb = await _context.Productos.FindAsync(det.ProductoID);
                    if (productoDb == null) throw new Exception($"Producto ID {det.ProductoID} no existe.");

                    decimal stockAnterior = productoDb.StockActual;

                    // Insertar en la tabla Detalle
                    var nuevoDetalle = new CompraDetalle
                    {
                        CompraID = nuevaCompra.CompraID,
                        ProductoID = det.ProductoID,
                        Cantidad = det.Cantidad,
                        PrecioCostoUnitario = det.PrecioCostoUnitario,
                        SubTotal = det.SubTotal
                    };
                    _context.CompraDetalles.Add(nuevoDetalle);

                    // LOGÍSTICA: Actualizar el Stock del Producto
                    productoDb.StockActual += det.Cantidad;
                    productoDb.PrecioCosto = det.PrecioCostoUnitario;

                    itemsCompra.Add((productoDb, det.Cantidad, stockAnterior));
                }

                await _context.SaveChangesAsync();

                // Registrar en Kardex con el CompraID ya generado
                foreach (var (prod, cantidad, stockAnterior) in itemsCompra)
                {
                    _context.MovimientosInventario.Add(new MovimientoInventario
                    {
                        Fecha = DateTime.UtcNow,
                        Tipo = "ENTRADA",
                        ProductoID = prod.ProductoID,
                        Cantidad = cantidad,
                        UsuarioID = parsedUsuarioId ?? 1,
                        StockAnterior = stockAnterior,
                        StockNuevo = stockAnterior + cantidad,
                        ReferenciaID = nuevaCompra.CompraID,
                        Observaciones = $"Compra #{nuevaCompra.CompraID} - {nuevaCompra.NumeroComprobante}"
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, mensaje = "Compra registrada e inventario sumado con éxito." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                return StatusCode(500, new { success = false, mensaje = "Error crítico al registrar compra: " + ex.Message + ". DETALLE: " + innerMsg });
            }
        }
    }
}
