using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;
using BCrypt.Net;


namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMINISTRADOR")] // Protege todos los métodos para que solo el Admin ingrese
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            // Devolvemos todos los usuarios excepto aquellos datos sensibles
            var usuarios = await _context.Usuarios
                .Select(u => new 
                {
                    u.UsuarioID,
                    u.DNI,
                    u.NombreCompleto,
                    u.Correo,
                    u.Rol,
                    u.Activo,
                    u.FechaCreacion
                })
                .ToListAsync();

            return Ok(new { success = true, data = usuarios });
        }

        // PUT: api/Usuarios/{id} (Para Editar nombre, DNI o Rol)
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuario(int id, [FromBody] Usuario updateData)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { success = false, mensaje = "Usuario no encontrado" });

            // Verificamos que el DNI o Correo no colisionen con otro usuario diferente
            if (await _context.Usuarios.AnyAsync(u => u.Correo == updateData.Correo && u.UsuarioID != id))
                return BadRequest(new { success = false, mensaje = "El correo ya pertenece a otro usuario." });

            if (await _context.Usuarios.AnyAsync(u => u.DNI == updateData.DNI && u.UsuarioID != id))
                return BadRequest(new { success = false, mensaje = "El DNI ya pertenece a otro usuario." });

            usuario.DNI = updateData.DNI;
            usuario.NombreCompleto = updateData.NombreCompleto;
            usuario.Correo = updateData.Correo;
            usuario.Rol = updateData.Rol.ToUpper();
            
            // Si el admin envía una contraseña nueva, la hasheamos
            if (!string.IsNullOrEmpty(updateData.PasswordHash)) 
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateData.PasswordHash);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, mensaje = "Usuario actualizado exitosamente." });
        }

        // PATCH: api/Usuarios/5/toggle-activo (Para desactivar o activar)
        [HttpPatch("{id}/toggle-activo")]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { success = false, mensaje = "Usuario no encontrado" });

            // Invertir el estado
            usuario.Activo = !usuario.Activo;
            
            await _context.SaveChangesAsync();
            string estadoTxt = usuario.Activo ? "activado" : "desactivado";
            return Ok(new { success = true, mensaje = $"Usuario {estadoTxt} exitosamente." });
        }
    }
}
