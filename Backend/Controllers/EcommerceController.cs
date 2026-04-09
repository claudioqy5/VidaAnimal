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
    }
}
