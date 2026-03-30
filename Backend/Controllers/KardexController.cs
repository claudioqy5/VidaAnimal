using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VidaAnimal.API.Data;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/kardex")]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class KardexController : ControllerBase
    {
        private readonly AppDbContext _context;
        public KardexController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetMovimientos()
        {
            var movimientos = await _context.MovimientosInventario
                .Include(m => m.Producto)
                .Include(m => m.Usuario)
                .OrderByDescending(m => m.Fecha)
                .Select(m => new {
                    m.MovimientoID,
                    m.Fecha,
                    m.Tipo,
                    m.ProductoID,
                    ProductoNombre = m.Producto != null ? m.Producto.Nombre : "Desconocido",
                    m.Cantidad,
                    m.StockAnterior,
                    m.StockNuevo,
                    m.ReferenciaID,
                    m.Observaciones,
                    m.UsuarioID,
                    UsuarioNombre = m.Usuario != null ? m.Usuario.NombreCompleto : "Sistema"
                })
                .ToListAsync();
            return Ok(new { success = true, data = movimientos });
        }
    }
}
