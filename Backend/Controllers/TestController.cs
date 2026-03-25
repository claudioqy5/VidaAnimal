using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using VidaAnimal.API.Data;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("conexion")]
        public async Task<IActionResult> ProbarConexion()
        {
            try
            {
                // Intenta conectarse a la Base de Datos usando tu cadena de conexión
                bool puedeConectarse = await _context.Database.CanConnectAsync();

                if (puedeConectarse)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        mensaje = "¡Conexión exitosa a SQL Server! La Base de Datos está lista." 
                    });
                }
                else
                {
                    return StatusCode(500, new 
                    { 
                        success = false, 
                        mensaje = "No se pudo conectar a la base de datos SQL Server. Revisa appsettings.json." 
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    mensaje = "Error intentando conectar: " + ex.Message 
                });
            }
        }
    }
}
