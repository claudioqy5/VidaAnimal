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

                // 1. Estadísticas BASADAS EN DETALLES (Subtotales aislados para margen exacto)
                var todosLosDetalles = await _context.VentaDetalles
                    .Include(d => d.Venta)
                    .Where(d => d.Venta != null && d.Venta.Estado == "Completada")
                    .ToListAsync();

                // Totales de Hoy
                var detallesHoy = todosLosDetalles.Where(d => d.Venta.Fecha.Date == hoy.Date).ToList();
                var ventasHoy = detallesHoy.Sum(d => d.SubTotal);
                var gananciaHoy = detallesHoy.Sum(d => (decimal?)d.Ganancia) ?? 0;

                // Totales Históricos
                var ventasHistoricas = todosLosDetalles.Sum(d => d.SubTotal);
                var gananciaHistorica = todosLosDetalles.Sum(d => (decimal?)d.Ganancia) ?? 0;

                // 2. Gráfico Semanal (L-D)
                var graficoVentas = new List<object>();
                var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Monday);
                if (hoy.DayOfWeek == DayOfWeek.Sunday) inicioSemana = hoy.AddDays(-6);

                for (int i = 0; i < 7; i++)
                {
                    var fecha = inicioSemana.AddDays(i);
                    var dataDia = todosLosDetalles.Where(v => v.Venta.Fecha.Date == fecha.Date).ToList();
                    graficoVentas.Add(new {
                        dia = fecha.ToString("dddd", new CultureInfo("es-ES")),
                        fecha = fecha.ToString("dd/MM"),
                        totalVentas = dataDia.Sum(v => v.SubTotal),
                        totalGanancia = dataDia.Sum(v => (decimal?)v.Ganancia) ?? 0
                    });
                }

                // 3. Gráfico Mensual
                var graficoMensual = new List<object>();
                var inicioMes = new DateTime(anioActual, mesActual, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                for (int i = 0; i < 5; i++)
                {
                    var sInicio = inicioMes.AddDays(i * 7);
                    if (sInicio > finMes) break;
                    var sFin = sInicio.AddDays(6);
                    if (sFin > finMes) sFin = finMes;

                    var dataSemana = todosLosDetalles.Where(v => v.Venta.Fecha.Date >= sInicio.Date && v.Venta.Fecha.Date <= sFin.Date).ToList();
                    graficoMensual.Add(new {
                        semana = $"S{i + 1}",
                        rango = $"{sInicio:dd/MM}-{sFin:dd/MM}",
                        totalVentas = dataSemana.Sum(v => v.SubTotal),
                        totalGanancia = dataSemana.Sum(v => (decimal?)v.Ganancia) ?? 0
                    });
                }

                return Ok(new {
                    success = true,
                    stats = new {
                        ventasHoy,
                        gananciaHoy,
                        ventasSemana = todosLosDetalles.Where(v => v.Venta.Fecha >= inicioSemana && v.Venta.Fecha <= inicioSemana.AddDays(6)).Sum(v => v.SubTotal),
                        gananciaSemana = todosLosDetalles.Where(v => v.Venta.Fecha >= inicioSemana && v.Venta.Fecha <= inicioSemana.AddDays(6)).Sum(v => (decimal?)v.Ganancia) ?? 0,
                        ventasMes = ventasHistoricas,
                        gananciaMes = gananciaHistorica
                    },
                    graficoSemanal = graficoVentas,
                    graficoMensual,
                    stockBajo = await _context.Productos.Where(p => p.Activo && p.StockActual <= p.StockMinimo).OrderBy(p => p.StockActual).Take(5)
                        .Select(p => new { p.Nombre, p.StockActual, p.UnidadMedida }).ToListAsync()
                });
            } catch (Exception ex) {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
