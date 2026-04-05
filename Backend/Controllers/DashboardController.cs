using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VidaAnimal.API.Data;
using System.Globalization;

namespace VidaAnimal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly VidaAnimalContext _context;

        public DashboardController(VidaAnimalContext context)
        {
            _context = context;
        }

        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen()
        {
            var hoy = DateTime.Today;
            var mesActual = hoy.Month;
            var anioActual = hoy.Year;

            // 1. Estadísticas Generales (Usando SubTotal de detalles para precisión)
            var detallesHoy = await _context.VentaDetalles
                .Where(d => d.Venta != null && d.Venta.Fecha.Date == hoy && d.Venta.Estado != "Anulada")
                .ToListAsync();

            var ventasHoy = detallesHoy.Sum(d => d.SubTotal);
            var gananciaHoy = detallesHoy.Sum(d => (decimal?)d.Ganancia) ?? 0;

            var detallesMes = await _context.VentaDetalles
                .Where(d => d.Venta != null && d.Venta.Fecha.Month == mesActual && d.Venta.Fecha.Year == anioActual && d.Venta.Estado != "Anulada")
                .ToListAsync();

            var ventasMes = detallesMes.Sum(d => d.SubTotal);
            var gananciaMes = detallesMes.Sum(d => (decimal?)d.Ganancia) ?? 0;

            // 2. Gráfico Semanal (L-D)
            var graficoVentas = new List<object>();
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Monday);
            if (hoy.DayOfWeek == DayOfWeek.Sunday) inicioSemana = hoy.AddDays(-6);

            var ventasSemanaRaw = await _context.VentaDetalles
                .Where(d => d.Venta != null && d.Venta.Fecha >= inicioSemana && d.Venta.Estado != "Anulada")
                .Select(d => new { d.Venta.Fecha, d.SubTotal, d.Ganancia })
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var fecha = inicioSemana.AddDays(i);
                var dataDia = ventasSemanaRaw.Where(v => v.Fecha.Date == fecha.Date).ToList();

                graficoVentas.Add(new
                {
                    dia = fecha.ToString("dddd", new CultureInfo("es-ES")),
                    fecha = fecha.ToString("dd/MM"),
                    totalVentas = dataDia.Sum(v => v.SubTotal),
                    totalGanancia = dataDia.Sum(v => (decimal?)v.Ganancia) ?? 0
                });
            }

            // 3. Gráfico Mensual (Por Semanas)
            var graficoMensual = new List<object>();
            var inicioMes = new DateTime(anioActual, mesActual, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            for (int i = 0; i < 5; i++)
            {
                var sInicio = inicioMes.AddDays(i * 7);
                if (sInicio > finMes) break;
                var sFin = sInicio.AddDays(6);
                if (sFin > finMes) sFin = finMes;

                var dataSemana = detallesMes.Where(v => v.Venta.Fecha.Date >= sInicio.Date && v.Venta.Date <= sFin.Date).ToList();
                graficoMensual.Add(new
                {
                    semana = $"S{i + 1}",
                    rango = $"{sInicio:dd/MM}-{sFin:dd/MM}",
                    totalVentas = dataSemana.Sum(v => v.SubTotal),
                    totalGanancia = dataSemana.Sum(v => (decimal?)v.Ganancia) ?? 0
                });
            }

            // 4. Totales Semanales
            var ventasSemanaTotal = ventasSemanaRaw.Sum(v => v.SubTotal);
            var gananciaSemanaTotal = ventasSemanaRaw.Sum(v => (decimal?)v.Ganancia) ?? 0;

            // 5. Stock Bajo
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
                    ventasSemana = ventasSemanaTotal,
                    gananciaSemana = gananciaSemanaTotal,
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
