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
        public async Task<IActionResult> GetResumen()
        {
            try {
                // Cross-platform TimeZone handling (Windows vs Linux)
                TimeZoneInfo peruTimeZone;
                try {
                    peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                } catch {
                    try {
                        peruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");
                    } catch {
                        // Fallback fallback
                        peruTimeZone = TimeZoneInfo.Local; 
                    }
                }

                var ahoraPeru = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, peruTimeZone);
                var hoy = ahoraPeru.Date;
                var inicioSemana = hoy.AddDays(-(int)ahoraPeru.DayOfWeek + (int)DayOfWeek.Monday);
                if (ahoraPeru.DayOfWeek == DayOfWeek.Sunday) inicioSemana = hoy.AddDays(-6);
                
                var inicioMes = new DateTime(ahoraPeru.Year, ahoraPeru.Month, 1);
                
                // Prefilter from DB (since start of month or 7 days ago) to avoid loading everything
                var fechaLimitePeru = inicioMes < hoy.AddDays(-7) ? inicioMes : hoy.AddDays(-7);
                var fechaLimiteUTC = fechaLimitePeru.AddHours(1); // Small buffer

                var detallesQuery = await _context.VentaDetalles
                    .Include(d => d.Venta)
                    .Include(d => d.Producto)
                    .Where(d => d.Venta != null && d.Venta.Estado == "Completada" && d.Venta.Fecha >= fechaLimiteUTC)
                    .ToListAsync();

                // Group in memory with Peru conversion
                var data = detallesQuery.Select(d => new {
                    Detalle = d,
                    FechaPeru = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(d.Venta.Fecha, DateTimeKind.Utc), peruTimeZone)
                }).ToList();

                var detallesHoy = data.Where(d => d.FechaPeru.Date == hoy).ToList();
                var ventasHoyGroups = detallesHoy.GroupBy(d => d.Detalle.VentaID).ToList();

                // 1. STATS CORE
                var stats = new {
                    ventasHoy = detallesHoy.Sum(d => d.Detalle.SubTotal),
                    gananciaHoy = detallesHoy.Sum(d => (decimal?)d.Detalle.Ganancia) ?? 0,
                    transaccionesHoy = ventasHoyGroups.Count,
                    clientesHoy = detallesHoy.Select(d => d.Detalle.Venta.ClienteID).Distinct().Count(),
                    
                    ventasSemana = data.Where(d => d.FechaPeru.Date >= inicioSemana.Date).Sum(d => d.Detalle.SubTotal),
                    gananciaSemana = data.Where(d => d.FechaPeru.Date >= inicioSemana.Date).Sum(d => (decimal?)d.Detalle.Ganancia) ?? 0,
                    
                    ventasMes = data.Where(d => d.FechaPeru.Month == ahoraPeru.Month && d.FechaPeru.Year == ahoraPeru.Year).Sum(d => d.Detalle.SubTotal),
                    gananciaMes = data.Where(d => d.FechaPeru.Month == ahoraPeru.Month && d.FechaPeru.Year == ahoraPeru.Year).Sum(d => (decimal?)d.Detalle.Ganancia) ?? 0
                };

                // 2. INICIO: TOP PRODUCTOS HOY Y FLUJO
                var topProductosHoy = detallesHoy
                    .GroupBy(d => d.Detalle.Producto.Nombre)
                    .Select(g => new { producto = g.Key, total = g.Sum(d => d.Detalle.SubTotal), cantidad = g.Sum(d => d.Detalle.Cantidad) })
                    .OrderByDescending(x => x.total).Take(5).ToList();

                var flujoHoras = new List<object>();
                for (int i = 6; i <= 22; i++) {
                    var totalHora = detallesHoy.Where(d => d.FechaPeru.Hour == i).Sum(d => d.Detalle.SubTotal);
                    flujoHoras.Add(new { hora = i, total = totalHora });
                }

                // 3. DASHBOARD: RANKINGS SEMANAL/MENSUAL
                var topSemanal = data
                    .Where(d => d.FechaPeru.Date >= inicioSemana.Date)
                    .GroupBy(d => d.Detalle.Producto.Nombre)
                    .Select(g => new { nombre = g.Key, totalMonto = g.Sum(d => d.Detalle.SubTotal), totalUnidades = g.Sum(d => d.Detalle.Cantidad) })
                    .OrderByDescending(x => x.totalMonto).Take(10).ToList();

                var topMensual = data
                    .Where(d => d.FechaPeru.Month == ahoraPeru.Month && d.FechaPeru.Year == ahoraPeru.Year)
                    .GroupBy(d => d.Detalle.Producto.Nombre)
                    .Select(g => new { nombre = g.Key, totalMonto = g.Sum(d => d.Detalle.SubTotal), totalUnidades = g.Sum(d => d.Detalle.Cantidad) })
                    .OrderByDescending(x => x.totalMonto).Take(10).ToList();

                // 4. OTROS
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

                return Ok(new {
                    success = true,
                    stats,
                    topProductosHoy,
                    flujoHoras,
                    topSemanal,
                    topMensual,
                    topProveedores,
                    stockBajo,
                    graficoSemanal = GenerarGraficoSemanal(data, inicioSemana),
                    graficoMensual = GenerarGraficoMensual(data, ahoraPeru)
                });
            } catch (Exception ex) {
                return BadRequest(new { success = false, message = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        private List<object> GenerarGraficoSemanal(List<dynamic> data, DateTime inicio) {
            var lista = new List<object>();
            for (int i = 0; i < 7; i++) {
                var fecha = inicio.AddDays(i);
                var dDia = data.Where(d => d.FechaPeru.Date == fecha.Date).ToList();
                lista.Add(new {
                    dia = fecha.ToString("dddd", new CultureInfo("es-ES")),
                    fecha = fecha.ToString("dd/MM"),
                    totalVentas = dDia.Sum(d => (decimal)d.Detalle.SubTotal),
                    totalGanancia = dDia.Sum(d => (decimal?)d.Detalle.Ganancia) ?? 0
                });
            }
            return lista;
        }

        private List<object> GenerarGraficoMensual(List<dynamic> data, DateTime ahora) {
            var lista = new List<object>();
            var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
            for (int i = 0; i < 5; i++) {
                var sInicio = inicioMes.AddDays(i * 7);
                if (sInicio.Month != ahora.Month) break;
                var sFin = sInicio.AddDays(6);
                var dSem = data.Where(d => d.FechaPeru.Date >= sInicio.Date && d.FechaPeru.Date <= sFin.Date).ToList();
                lista.Add(new {
                    semana = $"Semana {i + 1}",
                    rango = $"{sInicio:dd/MM} - {sFin:dd/MM}",
                    totalVentas = dSem.Sum(d => (decimal)d.Detalle.SubTotal),
                    totalGanancia = dSem.Sum(d => (decimal?)d.Detalle.Ganancia) ?? 0
                });
            }
            return lista;
        }
    }
}
