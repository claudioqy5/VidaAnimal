using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;

namespace VidaAnimal.API.Controllers
{
    // DTO para recibir datos del cliente sin conflictos de fechas
    public class ClienteCreateDto
    {
        public string DocumentoIdentidad { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public string? FechaNacimiento { get; set; } // Recibimos como string para validar nosotros
        public bool Activo { get; set; } = true;
    }

    [ApiController]
    [Route("api/clientes")]
    // [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Clientes.OrderByDescending(c => c.FechaRegistro).ToListAsync();
            return Ok(new { success = true, data = clientes });
        }

        [HttpPost]
        public async Task<IActionResult> CrearCliente([FromBody] ClienteCreateDto dto)
        {
            try {
                var cliente = new Cliente
                {
                    DocumentoIdentidad = dto.DocumentoIdentidad,
                    NombreCompleto = dto.NombreCompleto,
                    Telefono = dto.Telefono,
                    Correo = dto.Correo,
                    Direccion = dto.Direccion,
                    Activo = dto.Activo,
                    FechaRegistro = DateTime.Now,
                    // Parseamos la fecha solo si viene un valor válido
                    FechaNacimiento = (!string.IsNullOrEmpty(dto.FechaNacimiento) && DateTime.TryParse(dto.FechaNacimiento, out var fecha))
                        ? fecha
                        : null
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, mensaje = "Cliente registrado con éxito.", data = cliente });
            } catch (Exception ex) {
                return BadRequest(new { success = false, mensaje = "Error: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarCliente(int id, [FromBody] Cliente cliente)
        {
            var c = await _context.Clientes.FindAsync(id);
            if (c == null) return NotFound();

            c.NombreCompleto = cliente.NombreCompleto;
            c.DocumentoIdentidad = cliente.DocumentoIdentidad;
            c.Telefono = cliente.Telefono;
            c.Correo = cliente.Correo;
            c.Direccion = cliente.Direccion;
            c.FechaNacimiento = cliente.FechaNacimiento;
            c.Activo = cliente.Activo;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, mensaje = "Cliente actualizado." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var c = await _context.Clientes.FindAsync(id);
            if (c == null) return NotFound();

            _context.Clientes.Remove(c);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, mensaje = "Cliente eliminado permanentemente." });
        }
    }
}
