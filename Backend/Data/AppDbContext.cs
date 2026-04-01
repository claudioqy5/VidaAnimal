using Microsoft.EntityFrameworkCore;
using VidaAnimal.API.Models;

namespace VidaAnimal.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; } // Agregado para el login
        public DbSet<Cliente> Clientes { get; set; } // Nuevo dominio Clientes
        public DbSet<Compra> Compras { get; set; }
        public DbSet<CompraDetalle> CompraDetalles { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<VentaDetalle> VentaDetalles { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioCosto)
                .HasColumnType("decimal(12,2)");

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioVenta)
                .HasColumnType("decimal(12,2)");
                
            modelBuilder.Entity<Producto>()
                .Property(p => p.StockActual)
                .HasColumnType("decimal(12,3)");
                
            modelBuilder.Entity<Producto>()
                .Property(p => p.StockMinimo)
                .HasColumnType("decimal(12,3)");

            modelBuilder.Entity<Producto>()
                .Property(p => p.PrecioMayorista)
                .HasColumnType("decimal(12,2)");

            modelBuilder.Entity<Producto>()
                .Property(p => p.CantidadMayorista)
                .HasColumnType("decimal(12,3)");

            // Trigger TR_ReducirStock fue eliminado. El backend gestiona el stock directamente.
        }
    }
}
