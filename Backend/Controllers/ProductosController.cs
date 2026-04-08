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
            // Calculamos la popularidad basada en la cantidad vendida en los últimos 30 días
            var fechaLimite = DateTime.Now.AddDays(-30);
            
            var productosPopularidad = await _context.VentaDetalles
                .Where(vd => vd.Venta != null && vd.Venta.Fecha >= fechaLimite && vd.Venta.Estado != "Anulada")
                .GroupBy(vd => vd.ProductoID)
                .Select(g => new { ProductoID = g.Key, CantidadVendida = g.Sum(vd => vd.Cantidad) })
                .ToListAsync();

            var todosLosProductos = await _context.Productos
                .Include(p => p.Especies)
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .ToListAsync();

            // Unimos los productos con su conteo de ventas y ordenamos
            var resultado = todosLosProductos
                .Select(p => new {
                    Producto = p,
                    Ventas = productosPopularidad.FirstOrDefault(pop => pop.ProductoID == p.ProductoID)?.CantidadVendida ?? 0
                })
                .OrderByDescending(x => x.Ventas) // Los más vendidos primero
                .ThenBy(x => x.Producto.Nombre)    // Luego por nombre alfabéticamente
                .Select(x => x.Producto)
                .ToList();

            return Ok(new { success = true, data = resultado });
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
                    PrecioMayorista = modelo.PrecioMayorista,
                    CantidadMayorista = modelo.CantidadMayorista,
                    NombreUnidadMayorista = modelo.NombreUnidadMayorista,
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    ImagenURL = rutaImagenBD,
                    CategoriaID = modelo.CategoriaID
                };

                // Procesar Especies Múltiples
                if (!string.IsNullOrEmpty(modelo.EspecieIdsJson))
                {
                    var ids = System.Text.Json.JsonSerializer.Deserialize<List<int>>(modelo.EspecieIdsJson);
                    if (ids != null)
                    {
                        nuevoProducto.Especies = await _context.Especies.Where(e => ids.Contains(e.EspecieID)).ToListAsync();
                    }
                }

                _context.Productos.Add(nuevoProducto);
                await _context.SaveChangesAsync();

                // Si se registró un stock inicial, crear la Compra y el Movimiento de Inventario Kardex
                if (nuevoProducto.StockActual > 0)
                {
                    // 1. Crear Compra (Para que salga en Historial de Compras)
                    var nuevaCompra = new Compra
                    {
                        ProveedorID = nuevoProducto.ProveedorID ?? 1, // Si no eligió proveedor, asume el ID 1 por defecto
                        NumeroComprobante = "INITIAL-" + nuevoProducto.ProductoID.ToString(),
                        FechaCompra = DateTime.Now,
                        Total = nuevoProducto.StockActual * nuevoProducto.PrecioCosto,
                    };

                    var compDetalle = new CompraDetalle
                    {
                        ProductoID = nuevoProducto.ProductoID,
                        Cantidad = nuevoProducto.StockActual,
                        PrecioCostoUnitario = nuevoProducto.PrecioCosto,
                        SubTotal = nuevoProducto.StockActual * nuevoProducto.PrecioCosto
                    };
                    nuevaCompra.Detalles.Add(compDetalle);
                    _context.Compras.Add(nuevaCompra);

                    // 2. Crear Movimiento en el Kardex
                    var claimUsuarioId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                    int? usuarioId = null;
                    if (int.TryParse(claimUsuarioId, out int uid)) usuarioId = uid;

                    var kardex = new MovimientoInventario
                    {
                        Fecha = DateTime.Now,
                        Tipo = "ENTRADA",
                        ProductoID = nuevoProducto.ProductoID,
                        Cantidad = nuevoProducto.StockActual,
                        UsuarioID = usuarioId ?? 1,
                        StockAnterior = 0,
                        StockNuevo = nuevoProducto.StockActual,
                        ReferenciaID = nuevoProducto.ProductoID,
                        Observaciones = "Carga de Stock Inicial al crear el producto"
                    };
                    _context.MovimientosInventario.Add(kardex);

                    await _context.SaveChangesAsync();
                }

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
            else if (modelo.EliminarImagen)
            {
                p.ImagenURL = null;
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
            p.PrecioMayorista = modelo.PrecioMayorista;
            p.CantidadMayorista = modelo.CantidadMayorista;
            p.NombreUnidadMayorista = modelo.NombreUnidadMayorista;
            p.CategoriaID = modelo.CategoriaID;

            // Procesar actualización de Especies (Muchos a Muchos)
            if (!string.IsNullOrEmpty(modelo.EspecieIdsJson))
            {
                var ids = System.Text.Json.JsonSerializer.Deserialize<List<int>>(modelo.EspecieIdsJson);
                if (ids != null)
                {
                    // Cargamos explícitamente para borrar y re-asignar
                    await _context.Entry(p).Collection(prod => prod.Especies).LoadAsync();
                    p.Especies.Clear();
                    p.Especies = await _context.Especies.Where(e => ids.Contains(e.EspecieID)).ToListAsync();
                }
            }
            else 
            {
                // Si viene vacío o nulo, quitamos todas las especies
                await _context.Entry(p).Collection(prod => prod.Especies).LoadAsync();
                p.Especies.Clear();
            }

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
