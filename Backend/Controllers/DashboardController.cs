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
                // 1. Manejo de Zona Horaria Perú
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
                var hoy = ahoraPeru.Date;
                var inicioSemana = hoy.AddDays(-(int)ahoraPeru.DayOfWeek + (int)DayOfWeek.Monday);
                if (ahoraPeru.DayOfWeek == DayOfWeek.Sunday) inicioSemana = hoy.AddDays(-6);
                var inicioMes = new DateTime(ahoraPeru.Year, ahoraPeru.Month, 1);

                // 2. Cargar Ventas del Mes (Cabeceras y Detalles)
                // Usamos un filtro de seguridad de 31 días para cubrir cualquier desfase
                var fechaLimiteUTC = ahoraPeru.AddDays(-32).ToUniversalTime();

                var ventasQuery = await _context.Ventas
                    .Include(v => v.VentaDetalles)
                        .ThenInclude(d => d.Producto)
                    .Where(v => v.Estado == "Completada" && v.Fecha >= fechaLimiteUTC)
                    .ToListAsync();

                // Procesamos en memoria para convertir a hora de Perú una sola vez
                var todasVentas = ventasQuery.Select(v => new {
                    Venta = v,
                    FechaPeru = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(v.Fecha, DateTimeKind.Utc), peruTimeZone)
                }).ToList();

                // 3. Filtrar por periodos según hora Perú
                var ventasHoy = todasVentas.Where(v => v.FechaPeru.Date == hoy).Select(v => v.Venta).ToList();
                var ventasSemana = todasVentas.Where(v => v.FechaPeru.Date >= inicioSemana.Date).Select(v => v.Venta).ToList();
                var ventasMes = todasVentas.Where(v => v.FechaPeru.Month == ahoraPeru.Month && v.FechaPeru.Year == ahoraPeru.Year).Select(v => v.Venta).ToList();

                // 4. Calcular KPIs (Stats)
                var stats = new {
                    ventasHoy = Math.Round(ventasHoy.Sum(v => v.Total), 2),
                    gananciaHoy = Math.Round(ventasHoy.Sum(v => v.VentaDetalles.Sum(d => d.Ganancia ?? 0)), 2),
                    transaccionesHoy = ventasHoy.Count,
                    clientesHoy = ventasHoy.Select(v => v.ClienteID).Distinct().Count(),

                    ventasSemana = Math.Round(ventasSemana.Sum(v => v.Total), 2),
                    gananciaSemana = Math.Round(ventasSemana.Sum(v => v.VentaDetalles.Sum(d => d.Ganancia ?? 0)), 2),

                    ventasMes = Math.Round(ventasMes.Sum(v => v.Total), 2),
                    gananciaMes = Math.Round(ventasMes.Sum(v => v.VentaDetalles.Sum(d => d.Ganancia ?? 0)), 2)
                };

                // 5. Rankings y Gráficos
                var topProductosHoy = ventasHoy
                    .SelectMany(v => v.VentaDetalles)
                    .GroupBy(d => d.Producto?.Nombre ?? "Desconocido")
                    .Select(g => new { producto = g.Key, total = Math.Round(g.Sum(d => d.SubTotal), 2), cantidad = g.Sum(d => d.Cantidad) })
                    .OrderByDescending(x => x.total).Take(5).ToList();

                var flujoHoras = new List<object>();
                for (int i = 6; i <= 22; i++) {
                    var totalHora = ventasHoy.Where(v => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(v.Fecha, DateTimeKind.Utc), peruTimeZone).Hour == i).Sum(v => v.Total);
                    flujoHoras.Add(new { hora = i, total = Math.Round(totalHora, 2) });
                }

                // Rankings del Dashboard
                var topSemanal = ventasSemana
                    .SelectMany(v => v.VentaDetalles)
                    .GroupBy(d => d.Producto?.Nombre ?? "Desconocido")
                    .Select(g => new { nombre = g.Key, totalMonto = Math.Round(g.Sum(d => d.SubTotal), 2), totalUnidades = g.Sum(d => d.Cantidad) })
                    .OrderByDescending(x => x.totalMonto).Take(10).ToList();

                var topMensual = ventasMes
                    .SelectMany(v => v.VentaDetalles)
                    .GroupBy(d => d.Producto?.Nombre ?? "Desconocido")
                    .Select(g => new { nombre = g.Key, totalMonto = Math.Round(g.Sum(d => d.SubTotal), 2), totalUnidades = g.Sum(d => d.Cantidad) })
                    .OrderByDescending(x => x.totalMonto).Take(10).ToList();

                // Otros
                var topProveedores = await _context.Compras
                    .Include(c => c.Proveedor)
                    .Where(c => c.FechaCompra.Month == ahoraPeru.Month && c.FechaCompra.Year == ahoraPeru.Year && c.Proveedor != null)
                    .GroupBy(c => c.Proveedor.Nombre)
                    .Select(g => new { nombre = g.Key, totalInvertido = Math.Round(g.Sum(c => c.Total), 2) })
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
                    graficoSemanal = GenerarGráficoSemanal(todasVentas, inicioSemana),
                    graficoMensual = GenerarGráficoMensual(todasVentas, ahoraPeru)
                });

            } catch (Exception ex) {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private List<object> GenerarGráficoSemanal(IEnumerable<dynamic> ventas, DateTime inicio) {
            var lista = new List<object>();
            for (int i = 0; i < 7; i++) {
                var fecha = inicio.AddDays(i);
                var ventasDia = ventas.Where(v => ((DateTime)v.FechaPeru).Date == fecha.Date).Select(v => v.Venta).ToList();
                lista.Add(new {
                    dia = fecha.ToString("dddd", new CultureInfo("es-ES")),
                    fecha = fecha.ToString("dd/MM"),
                    totalVentas = Math.Round(ventasDia.Sum(v => (decimal)v.Total), 2),
                    totalGanancia = Math.Round(ventasDia.Sum(v => (decimal)v.VentaDetalles.Sum(d => d.Ganancia ?? 0)), 2)
                });
            }
            return lista;
        }

        private List<object> GenerarGráficoMensual(IEnumerable<dynamic> ventas, DateTime ahora) {
            var lista = new List<object>();
            var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
            for (int i = 0; i < 5; i++) {
                var sInicio = inicioMes.AddDays(i * 7);
                if (sInicio.Month != ahora.Month) break;
                var sFin = sInicio.AddDays(6);
                var ventasSem = ventas.Where(v => ((DateTime)v.FechaPeru).Date >= sInicio.Date && ((DateTime)v.FechaPeru).Date <= sFin.Date).Select(v => v.Venta).ToList();
                lista.Add(new {
                    semana = $"Semana {i + 1}",
                    rango = $"{sInicio:dd/MM} - {sFin:dd/MM}",
                    totalVentas = Math.Round(ventasSem.Sum(v => (decimal)v.Total), 2),
                    totalGanancia = Math.Round(ventasSem.Sum(v => (decimal)v.VentaDetalles.Sum(d => d.Ganancia ?? 0)), 2)
                });
            }
            return lista;
        }
    }
}
