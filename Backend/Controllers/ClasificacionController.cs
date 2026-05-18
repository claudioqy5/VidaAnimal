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

        [HttpPut("especies/{id}")]
        public async Task<IActionResult> ActualizarEspecie(int id, [FromBody] Especie modelo)
        {
            if (id != modelo.EspecieID) return BadRequest(new { success = false, message = "ID mismatch" });
            _context.Entry(modelo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = modelo });
        }

        [HttpDelete("especies/{id}")]
        public async Task<IActionResult> EliminarEspecie(int id)
        {
            var especie = await _context.Especies.FindAsync(id);
            if (especie == null) return NotFound(new { success = false, message = "Especie no encontrada" });
            
            // Soft delete
            especie.Activo = false;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Especie eliminada (inactivada)" });
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

        [HttpPut("categorias/{id}")]
        public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] Categoria modelo)
        {
            if (id != modelo.CategoriaID) return BadRequest(new { success = false, message = "ID mismatch" });
            _context.Entry(modelo).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, data = modelo });
        }

        [HttpDelete("categorias/{id}")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound(new { success = false, message = "Categoría no encontrada" });
            
            // Soft delete
            categoria.Activo = false;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Categoría eliminada (inactivada)" });
        }
    }
}
