using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VidaAnimal.API.Data;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            var hoy = DateTime.Now.Date;
            var mesActual = hoy.Month;
            var anioActual = hoy.Year;

            // 1. Estadísticas rápidas
            var ventasHoy = await _context.Ventas
                .Where(v => v.Fecha.Date == hoy && v.Estado != "Anulada")
                .SumAsync(v => v.Total);

            var gananciaHoy = await _context.VentaDetalles
                .Where(d => d.Venta != null && d.Venta.Fecha.Date == hoy && d.Venta.Estado != "Anulada")
                .SumAsync(d => (decimal?)d.Ganancia) ?? 0;

            var ventasMes = await _context.Ventas
                .Where(v => v.Fecha.Month == mesActual && v.Fecha.Year == anioActual && v.Estado != "Anulada")
                .SumAsync(v => v.Total);

            var gananciaMes = await _context.VentaDetalles
                .Where(d => d.Venta != null && d.Venta.Fecha.Month == mesActual && d.Venta.Fecha.Year == anioActual && d.Venta.Estado != "Anulada")
                .SumAsync(d => (decimal?)d.Ganancia) ?? 0;

            // 2. Gráfico Semanal (Lunes a Domingo)
            // Calculamos el inicio de la semana (Lunes)
            int diff = (7 + (hoy.DayOfWeek - DayOfWeek.Monday)) % 7;
            var inicioSemana = hoy.AddDays(-1 * diff).Date;
            var finSemana = inicioSemana.AddDays(7).AddTicks(-1);

            var ventasSemanalesRaw = await _context.VentaDetalles
                .Where(d => d.Venta != null && d.Venta.Fecha >= inicioSemana && d.Venta.Fecha <= finSemana && d.Venta.Estado != "Anulada")
                .Select(d => new { d.Venta.Fecha, d.SubTotal, d.Ganancia })
                .ToListAsync();

            var diasSemana = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
            var graficoVentas = new List<object>();

            for (int i = 0; i < 7; i++)
            {
                var fechaDia = inicioSemana.AddDays(i);
                var dataDia = ventasSemanalesRaw.Where(v => v.Fecha.Date == fechaDia).ToList();
                
                graficoVentas.Add(new
                {
                    dia = diasSemana[i],
                    fecha = fechaDia.ToString("dd/MM"),
                    totalVentas = dataDia.Sum(v => v.SubTotal),
                    totalGanancia = dataDia.Sum(v => (decimal?)v.Ganancia) ?? 0
                });
            }

            // 3. Gráfico Mensual (Agrupado por Semanas)
            var graficoMensual = new List<object>();
            var inicioMes = new DateTime(anioActual, mesActual, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var ventasMensualesRaw = await _context.VentaDetalles
                .Where(d => d.Venta != null && d.Venta.Fecha >= inicioMes && d.Venta.Fecha <= finMes && d.Venta.Estado != "Anulada")
                .Select(d => new { d.Venta.Fecha, d.SubTotal, d.Ganancia })
                .ToListAsync();

            // Agrupamos por semana (aprox 4-5 semanas)
            for (int i = 0; i < 5; i++)
            {
                var sInicio = inicioMes.AddDays(i * 7);
                if (sInicio > finMes) break;
                var sFin = sInicio.AddDays(6);
                if (sFin > finMes) sFin = finMes;

                var dataSemana = ventasMensualesRaw.Where(v => v.Fecha.Date >= sInicio.Date && v.Fecha.Date <= sFin.Date).ToList();
                graficoMensual.Add(new
                {
                    semana = $"S{i + 1}",
                    rango = $"{sInicio:dd/MM}-{sFin:dd/MM}",
                    totalVentas = dataSemana.Sum(v => v.SubTotal),
                    totalGanancia = dataSemana.Sum(v => (decimal?)v.Ganancia) ?? 0
                });
            }

            // 4. Stock Bajo
            var stockBajo = await _context.Productos
                .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
                .OrderBy(p => p.StockActual)
                .Take(5)
                .Select(p => new { p.Nombre, p.StockActual, p.UnidadMedida })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                stats = new
                {
                    ventasHoy,
                    gananciaHoy,
                    ventasMes,
                    gananciaMes
                },
                graficoSemanal = graficoVentas,
                graficoMensual,
                stockBajo
            });
        }
    }
}
