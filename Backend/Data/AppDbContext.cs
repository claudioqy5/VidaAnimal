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
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; } // Agregado para el login

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
        }
    }
}
