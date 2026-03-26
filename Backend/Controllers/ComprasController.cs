using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;
using VidaAnimal.API.DTOs;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var compras = await _context.Compras.OrderByDescending(c => c.FechaCompra).ToListAsync();
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
                // 1. Crear el encabezado de la Compra
                var nuevaCompra = new Compra
                {
                    ProveedorID = modelo.ProveedorID,
                    NumeroComprobante = modelo.NumeroComprobante,
                    FechaCompra = DateTime.Now,
                    Total = modelo.Total,
                    UsuarioID = null // Se corregirá a futuro con el token real
                };

                _context.Compras.Add(nuevaCompra);
                await _context.SaveChangesAsync();

                // 2. Procesar Detalles y Actualizar Stock Físico
                foreach (var det in modelo.Detalles)
                {
                    var productoDb = await _context.Productos.FindAsync(det.ProductoID);
                    if (productoDb == null) throw new Exception($"Producto ID {det.ProductoID} no existe.");

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
                    // Opcional: Actualizar el Precio Costo oficial del producto a la alza/baja (último costo)
                    productoDb.PrecioCosto = det.PrecioCostoUnitario; 
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
