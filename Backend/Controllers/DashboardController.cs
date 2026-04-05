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
                var hoy = DateTime.Today;
                var mesActual = hoy.Month;
                var anioActual = hoy.Year;

                // 1. Datos Base
                var todosLosDetalles = await _context.VentaDetalles
                    .Include(d => d.Venta)
                    .Include(d => d.Producto)
                    .Where(d => d.Venta != null && d.Venta.Estado == "Completada")
                    .ToListAsync();

                // 2. Gráficos
                var graficoSemanal = new List<object>();
                var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Monday);
                if (hoy.DayOfWeek == DayOfWeek.Sunday) inicioSemana = hoy.AddDays(-6);

                for (int i = 0; i < 7; i++)
                {
                    var fecha = inicioSemana.AddDays(i);
                    var dataDia = todosLosDetalles.Where(v => v.Venta.Fecha.Date == fecha.Date).ToList();
                    graficoSemanal.Add(new {
                        dia = fecha.ToString("dddd", new CultureInfo("es-ES")),
                        fecha = fecha.ToString("dd/MM"),
                        totalVentas = dataDia.Sum(v => v.SubTotal),
                        totalGanancia = dataDia.Sum(v => (decimal?)v.Ganancia) ?? 0
                    });
                }

                var graficoMensual = new List<object>();
                var inicioMes = new DateTime(anioActual, mesActual, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                for (int i = 0; i < 5; i++)
                {
                    var sInicio = inicioMes.AddDays(i * 7);
                    if (sInicio > finMes) break;
                    var sFin = sInicio.AddDays(6);
                    if (sFin > finMes) sFin = finMes;

                    var vSem = todosLosDetalles.Where(d => d.Venta.Fecha.Date >= sInicio.Date && d.Venta.Fecha.Date <= sFin.Date).ToList();
                    graficoMensual.Add(new {
                        semana = $"Semana {i + 1}",
                        rango = $"{sInicio:dd/MM} - {sFin:dd/MM}",
                        totalVentas = vSem.Sum(v => v.SubTotal),
                        totalGanancia = vSem.Sum(v => (decimal?)v.Ganancia) ?? 0
                    });
                }

                // 3. Tops & Inventario
                var topSemanal = todosLosDetalles
                    .Where(d => d.Venta.Fecha >= inicioSemana && d.Producto != null)
                    .GroupBy(d => d.Producto.Nombre)
                    .Select(g => new { nombre = g.Key, totalMonto = g.Sum(d => d.SubTotal), totalUnidades = g.Sum(d => d.Cantidad) })
                    .OrderByDescending(x => x.totalMonto).Take(5).ToList();

                var topMensual = todosLosDetalles
                    .Where(d => d.Venta.Fecha >= inicioMes && d.Producto != null)
                    .GroupBy(d => d.Producto.Nombre)
                    .Select(g => new { nombre = g.Key, totalMonto = g.Sum(d => d.SubTotal), totalUnidades = g.Sum(d => d.Cantidad) })
                    .OrderByDescending(x => x.totalMonto).Take(5).ToList();

                // TOP Proveedores (Corregido con FechaCompra)
                var topProveedores = await _context.Compras
                    .Include(c => c.Proveedor)
                    .Where(c => c.FechaCompra.Month == mesActual && c.Proveedor != null)
                    .GroupBy(c => c.Proveedor.Nombre)
                    .Select(g => new { nombre = g.Key, totalInvertido = g.Sum(c => c.Total) })
                    .OrderByDescending(x => x.totalInvertido)
                    .Take(5).ToListAsync();

                return Ok(new {
                    success = true,
                    stats = new {
                        ventasHoy = todosLosDetalles.Where(d => d.Venta.Fecha.Date == hoy.Date).Sum(d => d.SubTotal),
                        gananciaHoy = todosLosDetalles.Where(d => d.Venta.Fecha.Date == hoy.Date).Sum(d => (decimal?)d.Ganancia) ?? 0,
                        ventasSemana = todosLosDetalles.Where(v => v.Venta.Fecha >= inicioSemana && v.Venta.Fecha <= inicioSemana.AddDays(6)).Sum(v => v.SubTotal),
                        gananciaSemana = todosLosDetalles.Where(v => v.Venta.Fecha >= inicioSemana && v.Venta.Fecha <= inicioSemana.AddDays(6)).Sum(v => (decimal?)v.Ganancia) ?? 0,
                        ventasMes = todosLosDetalles.Sum(d => d.SubTotal),
                        gananciaMes = todosLosDetalles.Sum(d => (decimal?)d.Ganancia) ?? 0
                    },
                    graficoSemanal,
                    graficoMensual,
                    topSemanal,
                    topMensual,
                    topProveedores,
                    stockBajo = await _context.Productos.Where(p => p.Activo && p.StockActual <= p.StockMinimo).OrderBy(p => p.StockActual).Take(10)
                        .Select(p => new { p.Nombre, p.StockActual, p.UnidadMedida, p.StockMinimo }).ToListAsync()
                });
            } catch (Exception ex) {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
