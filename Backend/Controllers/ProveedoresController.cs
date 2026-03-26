using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "ADMINISTRADOR")] // Descomentar para asegurar
    public class ProveedoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProveedoresController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Proveedores
        [HttpGet]
        public async Task<IActionResult> GetProveedores()
        {
            var proveedores = await _context.Proveedores.ToListAsync();
            return Ok(new { success = true, data = proveedores });
        }

        // POST: api/Proveedores
        [HttpPost]
        public async Task<IActionResult> RegistrarProveedor([FromBody] Proveedor nuevo)
        {
            if (string.IsNullOrWhiteSpace(nuevo.Nombre))
                return BadRequest(new { success = false, mensaje = "El nombre es obligatorio." });

            nuevo.Activo = true; // Por defecto activo
            _context.Proveedores.Add(nuevo);
            await _context.SaveChangesAsync();
            
            return Ok(new { success = true, mensaje = "Proveedor registrado exitosamente.", data = nuevo });
        }

        // PUT: api/Proveedores/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarProveedor(int id, [FromBody] Proveedor updateData)
        {
            var p = await _context.Proveedores.FindAsync(id);
            if (p == null) return NotFound(new { success = false, mensaje = "Proveedor no encontrado" });

            p.Nombre = updateData.Nombre;
            p.Email = updateData.Email;
            p.Telefono = updateData.Telefono;
            p.Direccion = updateData.Direccion;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, mensaje = "Proveedor actualizado." });
        }

        // PATCH: api/Proveedores/{id}/toggle-activo
        [HttpPatch("{id}/toggle-activo")]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var p = await _context.Proveedores.FindAsync(id);
            if (p == null) return NotFound(new { success = false, mensaje = "Proveedor no encontrado" });

            p.Activo = !p.Activo;
            await _context.SaveChangesAsync();
            string estadoTxt = p.Activo ? "activado" : "desactivado";
            return Ok(new { success = true, mensaje = $"Proveedor {estadoTxt}." });
        }
    }
}
