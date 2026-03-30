using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System;
using VidaAnimal.API.DTOs;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            // Omitimos include de relaciones para hacer la petición rápida y sencilla por ahora
            var productos = await _context.Productos.ToListAsync();
            return Ok(new { success = true, data = productos });
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromForm] ProductoCreateDTO modelo)
        {
            try
            {
                string? rutaImagenBD = null;

                if (modelo.ImagenFoto != null && modelo.ImagenFoto.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "productos");
                    
                    if (!Directory.Exists(uploadsFolder)) 
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string nombreArchivoUnico = Guid.NewGuid().ToString() + "_" + modelo.ImagenFoto.FileName;
                    string rutaCompletaDisco = Path.Combine(uploadsFolder, nombreArchivoUnico);

                    using (var fileStream = new FileStream(rutaCompletaDisco, FileMode.Create))
                    {
                        await modelo.ImagenFoto.CopyToAsync(fileStream);
                    }

                    rutaImagenBD = $"/uploads/productos/{nombreArchivoUnico}";
                }

                var nuevoProducto = new Producto
                {
                    Codigo = modelo.Codigo,
                    Nombre = modelo.Nombre,
                    Descripcion = modelo.Descripcion,
                    ProveedorID = modelo.ProveedorID,
                    UnidadMedida = modelo.UnidadMedida,
                    PrecioCosto = modelo.PrecioCosto,
                    PrecioVenta = modelo.PrecioVenta,
                    StockActual = modelo.StockActual, 
                    StockMinimo = modelo.StockMinimo,
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    ImagenURL = rutaImagenBD
                };

                _context.Productos.Add(nuevoProducto);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, mensaje = "Producto creado.", data = nuevoProducto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, mensaje = "Error al crear producto: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditarProducto(int id, [FromForm] ProductoCreateDTO modelo)
        {
            var p = await _context.Productos.FindAsync(id);
            if (p == null) return NotFound(new { success = false, mensaje = "No encontrado" });

            if (modelo.ImagenFoto != null && modelo.ImagenFoto.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "productos");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string nm = Guid.NewGuid().ToString() + "_" + modelo.ImagenFoto.FileName;
                string rc = Path.Combine(uploadsFolder, nm);

                using (var fs = new FileStream(rc, FileMode.Create))
                {
                    await modelo.ImagenFoto.CopyToAsync(fs);
                }
                p.ImagenURL = $"/uploads/productos/{nm}";
            }

            p.Codigo = modelo.Codigo;
            p.Nombre = modelo.Nombre;
            p.Descripcion = modelo.Descripcion;
            p.ProveedorID = modelo.ProveedorID;
            p.UnidadMedida = modelo.UnidadMedida;
            p.PrecioCosto = modelo.PrecioCosto;
            p.PrecioVenta = modelo.PrecioVenta;
            p.StockActual = modelo.StockActual;
            p.StockMinimo = modelo.StockMinimo;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, mensaje = "Actualizado" });
        }

        [HttpPatch("{id}/toggle-activo")]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var p = await _context.Productos.FindAsync(id);
            if (p == null) return NotFound(new { success = false });

            p.Activo = !p.Activo;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost("{id}/eliminar")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> EliminarProducto(int id, [FromBody] DeleteVerifyRequest req)
        {
            var claimUsuarioId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (claimUsuarioId == null || !int.TryParse(claimUsuarioId, out int uid)) 
                return Unauthorized(new { success = false, mensaje = "Sesión inválida." });

            var usuario = await _context.Usuarios.FindAsync(uid);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(req.Password, usuario.PasswordHash))
            {
                return BadRequest(new { success = false, mensaje = "Contraseña de seguridad incorrecta. Operación denegada." });
            }

            var p = await _context.Productos.FindAsync(id);
            if (p == null) return NotFound(new { success = false, mensaje = "Producto no encontrado." });

            try 
            {
                _context.Productos.Remove(p);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, mensaje = "Producto eliminado definitivamente." });
            } 
            catch(Exception) 
            {
                return BadRequest(new { success = false, mensaje = "No se puede eliminar el producto porque ya tiene compras o ventas registradas en el historial. Intenta desactivarlo mejor." });
            }
        }
    }

    public class DeleteVerifyRequest 
    {
        public string Password { get; set; } = string.Empty;
    }
}
