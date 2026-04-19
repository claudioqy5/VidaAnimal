using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VidaAnimal.API.Data;
using System.Globalization;
using VidaAnimal.API.Models;
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
        public async Task<IActionResult> GetResumen([FromQuery] string? fecha = null, [FromQuery] string? semanaInicio = null)
        {
            try {
                TimeZoneInfo peruTimeZone;
                try {
                    peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                } catch {
                    try {
                        peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");
                    } catch {
                        peruTimeZone = TimeZoneInfo.Local; 
                    }
                }

                var ahoraPeru = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);

                // Día seleccionado (para Inicio). Por defecto: hoy.
                var hoy = DateTime.TryParse(fecha, out var fechaParsed) ? fechaParsed.Date : ahoraPeru.Date;

                // Inicio de semana seleccionada (para Dashboard). Por defecto: lunes de la semana actual.
                DateTime inicioSemana;
                if (DateTime.TryParse(semanaInicio, out var semanaInicioP)) {
                    inicioSemana = semanaInicioP.Date;
                } else {
                    inicioSemana = ahoraPeru.Date.AddDays(-(int)ahoraPeru.DayOfWeek + (int)DayOfWeek.Monday);
                    if (ahoraPeru.DayOfWeek == DayOfWeek.Sunday) inicioSemana = ahoraPeru.Date.AddDays(-6);
                }
                var finSemana = inicioSemana.AddDays(6);

                var inicioMes = new DateTime(ahoraPeru.Year, ahoraPeru.Month, 1);

                // Calcular fecha límite de consulta para abarcar tanto el día seleccionado como la semana seleccionada
                var fechaMasAntigua = new[] { hoy, inicioSemana, inicioMes }.Min();
                // Buffer amplio: incluimos un día antes Y validamos con FechaPeru.Date en memoria
                // Esto garantiza que las ventas guardadas en hora local o UTC sean capturadas
                var fechaLimiteUTC = fechaMasAntigua.AddDays(-2);
                var fechaLimiteMaxUTC = ahoraPeru.Date.AddDays(2); // techo superior

                var detallesQuery = await _context.VentaDetalles
                    .Include(d => d.Venta)
                    .Include(d => d.Producto)
                    .Where(d => d.Venta != null && d.Venta.Estado == "Completada" 
                        && d.Venta.Fecha >= fechaLimiteUTC
                        && d.Venta.Fecha <= fechaLimiteMaxUTC)
                    .ToListAsync();

                var data = detallesQuery.Select(d => new {
                    Detalle = d,
                    FechaPeru = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(d.Venta.Fecha, DateTimeKind.Utc), peruTimeZone)
                }).ToList();

                var detallesHoy = data.Where(d => d.FechaPeru.Date == hoy).ToList();
                var ventasHoyGroups = detallesHoy.GroupBy(d => d.Detalle.VentaID).Select(g => g.First().Detalle.Venta).ToList();

                var ventasSemanaGroups = data.Where(d => d.FechaPeru.Date >= inicioSemana.Date && d.FechaPeru.Date <= finSemana.Date).GroupBy(d => d.Detalle.VentaID).Select(g => g.First().Detalle.Venta).ToList();
                var ventasMesGroups = data.Where(d => d.FechaPeru.Month == ahoraPeru.Month && d.FechaPeru.Year == ahoraPeru.Year).GroupBy(d => d.Detalle.VentaID).Select(g => g.First().Detalle.Venta).ToList();

                var stats = new {
                    ventasHoy = ventasHoyGroups.Sum(v => v.Total),
                    gananciaHoy = detallesHoy.Sum(d => d.Detalle.Ganancia ?? 0) - ventasHoyGroups.Sum(v => v.Descuento),
                    transaccionesHoy = ventasHoyGroups.Count,
                    clientesHoy = detallesHoy.Select(d => d.Detalle.Venta.ClienteID).Distinct().Count(),
                    
                    ventasSemana = ventasSemanaGroups.Sum(v => v.Total),
                    gananciaSemana = data.Where(d => d.FechaPeru.Date >= inicioSemana.Date && d.FechaPeru.Date <= finSemana.Date).Sum(d => d.Detalle.Ganancia ?? 0) - ventasSemanaGroups.Sum(v => v.Descuento),
                    
                    ventasMes = ventasMesGroups.Sum(v => v.Total),
                    gananciaMes = data.Where(d => d.FechaPeru.Month == ahoraPeru.Month && d.FechaPeru.Year == ahoraPeru.Year).Sum(d => d.Detalle.Ganancia ?? 0) - ventasMesGroups.Sum(v => v.Descuento)
                };

                var topProductosHoy = detallesHoy
                    .GroupBy(d => d.Detalle.Producto.Nombre)
                    .Select(g => new { producto = g.Key, total = g.Sum(d => d.Detalle.SubTotal), cantidad = g.Sum(d => d.Detalle.Cantidad) })
                    .OrderByDescending(x => x.total).Take(5).ToList();

                var flujoHoras = new List<object>();
                for (int i = 6; i <= 22; i++) {
                    // Usamos FechaPeru.Hour para respetar la zona horaria de Perú
                    var totalHora = detallesHoy.Where(d => d.FechaPeru.Hour == i).Sum(d => (decimal)d.Detalle.SubTotal);
                    flujoHoras.Add(new { hora = i, total = totalHora });
                }

                var topSemanal = data
                    .Where(d => d.FechaPeru.Date >= inicioSemana.Date && d.FechaPeru.Date <= finSemana.Date)
                    .GroupBy(d => d.Detalle.Producto.Nombre)
                    .Select(g => new { nombre = g.Key, totalMonto = g.Sum(d => d.Detalle.SubTotal), totalUnidades = g.Sum(d => d.Detalle.Cantidad) })
                    .OrderByDescending(x => x.totalMonto).Take(10).ToList();

                var topMensual = data
                    .Where(d => d.FechaPeru.Month == ahoraPeru.Month && d.FechaPeru.Year == ahoraPeru.Year)
                    .GroupBy(d => d.Detalle.Producto.Nombre)
                    .Select(g => new { nombre = g.Key, totalMonto = g.Sum(d => d.Detalle.SubTotal), totalUnidades = g.Sum(d => d.Detalle.Cantidad) })
                    .OrderByDescending(x => x.totalMonto).Take(10).ToList();

                var topProveedores = await _context.Compras
                    .Include(c => c.Proveedor)
                    .Where(c => c.FechaCompra.Month == ahoraPeru.Month && c.FechaCompra.Year == ahoraPeru.Year && c.Proveedor != null)
                    .GroupBy(c => c.Proveedor.Nombre)
                    .Select(g => new { nombre = g.Key, totalInvertido = g.Sum(c => c.Total) })
                    .OrderByDescending(x => x.totalInvertido).Take(5).ToListAsync();

                var stockBajo = await _context.Productos
                    .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
                    .OrderBy(p => p.StockActual)
                    .Select(p => new { p.Nombre, p.StockActual, p.UnidadMedida, p.StockMinimo })
                    .Take(10).ToListAsync();

                // Ganancia histórica total del sistema (todo el tiempo, directo en BD)
                var sumaGananciaHistorica = await _context.VentaDetalles
                    .Where(d => d.Venta != null && d.Venta.Estado == "Completada")
                    .SumAsync(d => d.Ganancia ?? 0);
                var sumaDescuentosHistorica = await _context.Ventas
                    .Where(v => v.Estado == "Completada")
                    .SumAsync(v => v.Descuento);
                var gananciaHistorica = sumaGananciaHistorica - sumaDescuentosHistorica;

                return Ok(new {
                    success = true,
                    stats,
                    gananciaHistorica,
                    topProductosHoy,
                    flujoHoras,
                    topSemanal,
                    topMensual,
                    topProveedores,
                    stockBajo,
                    graficoSemanal = GenerarGraficoSemanal(data, inicioSemana),
                    graficoMensual = GenerarGraficoMensual(data, ahoraPeru),
                    semanaInicioUsada = inicioSemana.ToString("yyyy-MM-dd"),
                    fechaUsada = hoy.ToString("yyyy-MM-dd")
                });
            } catch (Exception ex) {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private List<object> GenerarGraficoSemanal(IEnumerable<dynamic> data, DateTime inicio) {
            var lista = new List<object>();
            foreach (var i in Enumerable.Range(0, 7)) {
                var fecha = inicio.AddDays(i);
                var itemsDia = data.Where(d => ((DateTime)d.FechaPeru).Date == fecha.Date).ToList();
                
                // Sumamos el total acumulado de las ventas únicas de ese día
                var ventasUnicas = itemsDia.GroupBy(d => (int)d.Detalle.VentaID).Select(g => g.First().Detalle.Venta);

                lista.Add(new {
                    dia = fecha.ToString("dddd", new CultureInfo("es-ES")),
                    fecha = fecha.ToString("dd/MM"),
                    totalVentas = ventasUnicas.Sum(v => (decimal)v.Total),
                    totalGanancia = itemsDia.Sum(d => (decimal)(d.Detalle.Ganancia ?? 0)) - ventasUnicas.Sum(v => (decimal)v.Descuento)
                });
            }
            return lista;
        }

        private List<object> GenerarGraficoMensual(IEnumerable<dynamic> data, DateTime ahora) {
            var lista = new List<object>();
            var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
            var finMes = new DateTime(ahora.Year, ahora.Month, DateTime.DaysInMonth(ahora.Year, ahora.Month));
            foreach (var i in Enumerable.Range(0, 5)) {
                var sInicio = inicioMes.AddDays(i * 7);
                if (sInicio.Month != ahora.Month) break;
                // Clavar sFin al último día del mes para no sangrar al mes siguiente
                var sFin = new[] { sInicio.AddDays(6), finMes }.Min();
                var itemsSem = data.Where(d => ((DateTime)d.FechaPeru).Date >= sInicio.Date && ((DateTime)d.FechaPeru).Date <= sFin.Date).ToList();

                var ventasUnicas = itemsSem.GroupBy(d => (int)d.Detalle.VentaID).Select(g => g.First().Detalle.Venta);

                lista.Add(new {
                    semana = $"Semana {i + 1}",
                    rango = $"{sInicio:dd/MM} - {sFin:dd/MM}",
                    totalVentas = ventasUnicas.Sum(v => (decimal)v.Total),
                    totalGanancia = itemsSem.Sum(d => (decimal)(d.Detalle.Ganancia ?? 0)) - ventasUnicas.Sum(v => (decimal)v.Descuento)
                });
            }
            return lista;
        }
    }
}
