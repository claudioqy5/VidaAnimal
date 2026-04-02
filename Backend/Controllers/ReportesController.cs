using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VidaAnimal.API.Data;

namespace VidaAnimal.API.Controllers
{
    [ApiController]
    [Route("api/reportes")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReportesController(AppDbContext ctx) => _context = ctx;

        // ─────────────────────────────────────────────
        // GET /api/reportes/inicio  →  Resumen del DÍA
        // ─────────────────────────────────────────────
        [HttpGet("inicio")]
        public async Task<IActionResult> GetInicio()
        {
            try {
                var hoy = DateTime.Today;
                var manana = hoy.AddDays(1);

                // Total vendido hoy
                var totalHoy = await _context.Ventas
                    .Where(v => v.Fecha >= hoy && v.Fecha < manana && v.Estado != "Anulada")
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                // Número de ventas hoy
                var numVentasHoy = await _context.Ventas
                    .Where(v => v.Fecha >= hoy && v.Fecha < manana && v.Estado != "Anulada")
                    .CountAsync();

                // Ventas por hora (para gráfico)
                var ventasPorHora = await _context.Ventas
                    .Where(v => v.Fecha >= hoy && v.Fecha < manana && v.Estado != "Anulada")
                    .GroupBy(v => v.Fecha.Hour)
                    .Select(g => new { Hora = g.Key, Total = g.Sum(v => v.Total) })
                    .OrderBy(x => x.Hora)
                    .ToListAsync();

                // Top 5 productos más vendidos hoy
                var topProductosHoy = await _context.VentaDetalles
                    .Include(d => d.Venta)
                    .Include(d => d.Producto)
                    .Where(d => d.Venta!.Fecha >= hoy && d.Venta.Fecha < manana && d.Venta.Estado != "Anulada")
                    .GroupBy(d => new { d.ProductoID, Name = d.Producto != null ? d.Producto.Nombre : "Producto" })
                    .Select(g => new {
                        ProductoID = g.Key.ProductoID,
                        Nombre = g.Key.Name,
                        TotalUnidades = g.Sum(d => d.Cantidad),
                        TotalMonto = g.Sum(d => d.SubTotal)
                    })
                    .OrderByDescending(x => x.TotalMonto)
                    .Take(5)
                    .ToListAsync();

                // Productos con stock bajo (menos del mínimo)
                var stockBajo = await _context.Productos
                    .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
                    .Select(p => new { p.ProductoID, p.Nombre, p.StockActual, p.StockMinimo })
                    .OrderBy(p => p.StockActual)
                    .Take(5)
                    .ToListAsync();

                // Clientes atendidos hoy
                var clientesHoy = await _context.Ventas
                    .Where(v => v.Fecha >= hoy && v.Fecha < manana && v.ClienteID != null && v.Estado != "Anulada")
                    .Select(v => v.ClienteID)
                    .Distinct()
                    .CountAsync();

                // Ventas por Método de Pago
                var ventasPorMetodo = await _context.Ventas
                    .Where(v => v.Fecha >= hoy && v.Fecha < manana && v.Estado != "Anulada")
                    .GroupBy(v => string.IsNullOrEmpty(v.MetodoPago) ? "Efectivo" : v.MetodoPago)
                    .Select(g => new { Metodo = g.Key, Total = g.Sum(v => v.Total) })
                    .OrderByDescending(x => x.Total)
                    .ToListAsync();

                return Ok(new {
                    success = true,
                    data = new {
                        totalHoy,
                        numVentasHoy,
                        clientesHoy,
                        ventasPorHora,
                        topProductosHoy,
                        stockBajo,
                        ventasPorMetodo
                    }
                });
            } catch (Exception ex) {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/reportes/dashboard  →  Resumen SEMANA y MES
        // ──────────────────────────────────────────────────────────
        [HttpGet("dashboard")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> GetDashboard()
        {
            try {
                var hoy = DateTime.Today;
                var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
                var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
                var finSemana = inicioSemana.AddDays(7);
                var finMes = inicioMes.AddMonths(1);

                // ── SEMANA ──
                var totalSemana = await _context.Ventas
                    .Where(v => v.Fecha >= inicioSemana && v.Fecha < finSemana && v.Estado != "Anulada")
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                var numVentasSemana = await _context.Ventas
                    .Where(v => v.Fecha >= inicioSemana && v.Fecha < finSemana && v.Estado != "Anulada")
                    .CountAsync();

                // Ventas por día de la semana - Agrupamos por fecha y proyectamos después para evitar fallos de traducción
                var ventasPorDiaRaw = await _context.Ventas
                    .Where(v => v.Fecha >= inicioSemana && v.Fecha < finSemana && v.Estado != "Anulada")
                    .Select(v => new { v.Fecha, v.Total })
                    .ToListAsync();

                var ventasPorDia = ventasPorDiaRaw
                    .GroupBy(v => v.Fecha.DayOfWeek)
                    .Select(g => new { Dia = (int)g.Key, Total = g.Sum(v => v.Total), Cantidad = g.Count() })
                    .OrderBy(x => x.Dia)
                    .ToList();

                // Top 5 productos semana
                var topProductosSemana = await _context.VentaDetalles
                    .Include(d => d.Venta).Include(d => d.Producto)
                    .Where(d => d.Venta!.Fecha >= inicioSemana && d.Venta.Fecha < finSemana && d.Venta.Estado != "Anulada")
                    .GroupBy(d => new { d.ProductoID, d.Producto!.Nombre })
                    .Select(g => new {
                        Nombre = g.Key.Nombre,
                        TotalUnidades = g.Sum(d => d.Cantidad),
                        TotalMonto = g.Sum(d => d.SubTotal)
                    })
                    .OrderByDescending(x => x.TotalMonto)
                    .Take(5)
                    .ToListAsync();

                // Top 5 clientes semana
                var topClientesSemana = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Where(v => v.Fecha >= inicioSemana && v.Fecha < finSemana && v.ClienteID != null && v.Estado != "Anulada")
                    .GroupBy(v => new { v.ClienteID, Nombre = v.Cliente != null ? v.Cliente.NombreCompleto : "Cliente" })
                    .Select(g => new {
                        ClienteID = g.Key.ClienteID,
                        Nombre = g.Key.Nombre,
                        NumVentas = g.Count(),
                        TotalGastado = g.Sum(v => v.Total)
                    })
                    .OrderByDescending(x => x.TotalGastado)
                    .Take(5)
                    .ToListAsync();

                // ── MES ──
                var totalMes = await _context.Ventas
                    .Where(v => v.Fecha >= inicioMes && v.Fecha < finMes && v.Estado != "Anulada")
                    .SumAsync(v => (decimal?)v.Total) ?? 0;

                var numVentasMes = await _context.Ventas
                    .Where(v => v.Fecha >= inicioMes && v.Fecha < finMes && v.Estado != "Anulada")
                    .CountAsync();

                // Top 5 productos mes
                var topProductosMes = await _context.VentaDetalles
                    .Include(d => d.Venta).Include(d => d.Producto)
                    .Where(d => d.Venta!.Fecha >= inicioMes && d.Venta.Fecha < finMes && d.Venta.Estado != "Anulada")
                    .GroupBy(d => new { d.ProductoID, Name = d.Producto != null ? d.Producto.Nombre : "Producto" })
                    .Select(g => new {
                        Nombre = g.Key.Name,
                        TotalUnidades = g.Sum(d => d.Cantidad),
                        TotalMonto = g.Sum(d => d.SubTotal)
                    })
                    .OrderByDescending(x => x.TotalMonto)
                    .Take(5)
                    .ToListAsync();

                // Proveedores ordenados por total comprado
                var proveedores = await _context.Compras
                    .Include(c => c.Proveedor)
                    .GroupBy(c => new { c.ProveedorID, Nombre = (c.Proveedor != null ? c.Proveedor.Nombre : "Proveedor") })
                    .Select(g => new {
                        ProveedorID = g.Key.ProveedorID,
                        Nombre = g.Key.Nombre,
                        TotalComprado = g.Sum(c => c.Total),
                        NumCompras = g.Count()
                    })
                    .OrderByDescending(x => x.TotalComprado)
                    .ToListAsync();

                return Ok(new {
                    success = true,
                    data = new {
                        semana = new { totalSemana, numVentasSemana, ventasPorDia, topProductosSemana, topClientesSemana },
                        mes = new { totalMes, numVentasMes, topProductosMes },
                        proveedores
                    }
                });
            } catch (Exception ex) {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
