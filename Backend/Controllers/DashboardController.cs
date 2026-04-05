using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VidaAnimal.API.Data;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace VidaAnimal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen()
        {
            try {
                var hoy = DateTime.Today;
                var mesActual = hoy.Month;
                var anioActual = hoy.Year;

                // 1. Estadísticas COHERENTES (Usando directamente la cabecera Ventas para el Total)
                var ventasFiltradas = await _context.Ventas
                    .Where(v => v.Estado == "Completada")
                    .ToListAsync();

                // Ventas de Hoy
                var ventasHoyTotal = ventasFiltradas.Where(v => v.Fecha.Date == hoy.Date).Sum(v => v.Total);

                // Ventas del Mes
                var ventasMesTotal = ventasFiltradas.Where(v => v.Fecha.Month == mesActual && v.Fecha.Year == anioActual).Sum(v => v.Total);

                // Ventas Históricas
                var ventasHistoricas = ventasFiltradas.Sum(v => v.Total);

                // 2. Ganancias (Necesitamos los detalles para esto)
                var detallesFiltrados = await _context.VentaDetalles
                    .Include(d => d.Venta)
                    .Where(d => d.Venta != null && d.Venta.Estado == "Completada")
                    .ToListAsync();

                var gananciaHoy = detallesFiltrados.Where(d => d.Venta.Fecha.Date == hoy.Date).Sum(d => (decimal?)d.Ganancia) ?? 0;
                var gananciaTotal = detallesFiltrados.Sum(d => (decimal?)d.Ganancia) ?? 0;

                // 3. Gráfico Semanal (L-D)
                var graficoVentas = new List<object>();
                var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Monday);
                if (hoy.DayOfWeek == DayOfWeek.Sunday) inicioSemana = hoy.AddDays(-6);

                for (int i = 0; i < 7; i++)
                {
                    var fecha = inicioSemana.AddDays(i);
                    var vDia = ventasFiltradas.Where(v => v.Fecha.Date == fecha.Date).Sum(v => v.Total);
                    var gDia = detallesFiltrados.Where(d => d.Venta.Fecha.Date == fecha.Date).Sum(d => (decimal?)d.Ganancia) ?? 0;

                    graficoVentas.Add(new {
                        dia = fecha.ToString("dddd", new CultureInfo("es-ES")),
                        fecha = fecha.ToString("dd/MM"),
                        totalVentas = vDia,
                        totalGanancia = gDia
                    });
                }

                return Ok(new {
                    success = true,
                    stats = new {
                        ventasHoy = ventasHoyTotal,
                        gananciaHoy,
                        ventasSemana = ventasFiltradas.Where(v => v.Fecha >= inicioSemana && v.Fecha <= inicioSemana.AddDays(6)).Sum(v => v.Total),
                        gananciaSemana = detallesFiltrados.Where(d => d.Venta.Fecha >= inicioSemana && d.Venta.Fecha <= inicioSemana.AddDays(6)).Sum(d => (decimal?)d.Ganancia) ?? 0,
                        ventasMes = ventasHistoricas,
                        gananciaMes = gananciaTotal
                    },
                    graficoSemanal = graficoVentas,
                    stockBajo = await _context.Productos.Where(p => p.Activo && p.StockActual <= p.StockMinimo).OrderBy(p => p.StockActual).Take(5)
                        .Select(p => new { p.Nombre, p.StockActual, p.UnidadMedida }).ToListAsync()
                });
            } catch (Exception ex) {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
