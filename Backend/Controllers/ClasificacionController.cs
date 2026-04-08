using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClasificacionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClasificacionController(AppDbContext context)
        {
            _context = context;
        }

        // --- ESPECIES ---
        [HttpGet("especies")]
        public async Task<IActionResult> GetEspecies()
        {
            var especies = await _context.Especies.Where(e => e.Activo).ToListAsync();
            return Ok(new { success = true, data = especies });
        }

        [HttpPost("especies")]
        public async Task<IActionResult> CrearEspecie([FromBody] Especie modelo)
        {
            _context.Especies.Add(modelo);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = modelo });
        }

        // --- CATEGORIAS ---
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _context.Categorias.Where(c => c.Activo).ToListAsync();
            return Ok(new { success = true, data = categorias });
        }

        [HttpPost("categorias")]
        public async Task<IActionResult> CrearCategoria([FromBody] Categoria modelo)
        {
            _context.Categorias.Add(modelo);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = modelo });
        }
    }
}
