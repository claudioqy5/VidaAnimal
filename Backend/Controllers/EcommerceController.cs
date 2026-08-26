using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Acceso totalmente público para la tienda
    public class EcommerceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EcommerceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Productos")]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _context.Productos
                .Include(p => p.Especies)
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .Select(p => new {
                    productoId = p.ProductoID,
                    nombre = p.Nombre,
                    descripcion = p.Descripcion,
                    precioVenta = p.PrecioVenta,
                    unidadMedida = p.UnidadMedida,
                    precioMayorista = p.PrecioMayorista,
                    cantidadMayorista = p.CantidadMayorista,
                    nombreUnidadMayorista = p.NombreUnidadMayorista,
                    imagenURL = p.ImagenURL,
                    categoriaId = p.CategoriaID,
                    categoria = p.Categoria != null ? new { p.Categoria.CategoriaID, p.Categoria.Nombre } : null,
                    especies = p.Especies.Select(e => new { especieId = e.EspecieID, nombre = e.Nombre }).ToList()
                })
                .ToListAsync();

            return Ok(new { success = true, data = productos });
        }

        [HttpGet("Categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.Categorias
                .Where(c => c.Activo)
                .Select(c => new { categoriaId = c.CategoriaID, nombre = c.Nombre })
                .ToListAsync();

            return Ok(new { success = true, data = categorias });
        }

        [HttpGet("Especies")]
        public async Task<IActionResult> GetEspecies()
        {
            var especies = await _context.Especies
                .Where(e => e.Activo)
                .Select(e => new { especieId = e.EspecieID, nombre = e.Nombre })
                .ToListAsync();

            return Ok(new { success = true, data = especies });
        }
        [HttpGet("ConsultaBoleta")]
        public async Task<IActionResult> ConsultaBoleta([FromQuery] string serie, [FromQuery] string numero)
        {
            if (string.IsNullOrWhiteSpace(serie) || string.IsNullOrWhiteSpace(numero))
                return BadRequest(new { success = false, message = "Debe proporcionar serie y número del comprobante." });

            // Normalizar: quitar espacios
            serie = serie.Trim().ToUpper();
            numero = numero.Trim();

            var venta = await _context.Ventas
                .Include(v => v.VentaDetalles)
                    .ThenInclude(d => d.Producto)
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => 
                    v.SerieComprobante == serie && 
                    v.NumeroComprobante == numero &&
                    v.SerieComprobante!.StartsWith("B")); // Solo boletas

            if (venta == null)
                return NotFound(new { success = false, message = "No se encontró la boleta con la serie y número proporcionados." });

            if (!venta.EnviadoSunat)
                return Ok(new { 
                    success = false, 
                    message = "Esta boleta no fue enviada a SUNAT correctamente." 
                });

            return Ok(new {
                success = true,
                data = new {
                    serie = venta.SerieComprobante,
                    numero = venta.NumeroComprobante,
                    fecha = venta.Fecha,
                    cliente = venta.Cliente != null ? new {
                        nombre = venta.Cliente.NombreCompleto,
                        documento = venta.Cliente.Documento
                    } : null,
                    total = venta.Total,
                    subTotal = venta.SubTotal,
                    estado = venta.Estado,
                    metodoPago = venta.MetodoPago,
                    sunatStatus = venta.SunatStatus,
                    sunatPdfUrl = venta.SunatPdfUrl,
                    sunatXmlUrl = venta.SunatXmlUrl,
                    sunatCdrUrl = venta.SunatCdrUrl,
                    detalles = venta.VentaDetalles.Select(d => new {
                        producto = d.Producto?.Nombre ?? "Producto",
                        cantidad = d.Cantidad,
                        precioUnitario = d.PrecioVentaUnitario,
                        subTotal = d.SubTotal
                    }).ToList()
                }
            });
        }
    }
}
