using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;
using VidaAnimal.API.DTOs;
using VidaAnimal.API.Data;
using VidaAnimal.API.Models;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                    CategoriaID = modelo.CategoriaID,
                    ProveedorID = modelo.ProveedorID,
                    UnidadMedida = modelo.UnidadMedida,
                    PrecioCosto = modelo.PrecioCosto,
                    PrecioVenta = modelo.PrecioVenta,
                    StockActual = 0, // Al crearlo el stock inicia en 0 (segun DB), luego se ingresa en Compras
                    StockMinimo = modelo.StockMinimo,
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    ImagenURL = rutaImagenBD
                };

                _context.Productos.Add(nuevoProducto);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, mensaje = "Producto creado exitosamente.", data = nuevoProducto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, mensaje = "Error al crear producto: " + ex.Message });
            }
        }
    }
}
