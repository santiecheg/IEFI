using Microsoft.EntityFrameworkCore;
using Verduleria.Entities;

namespace Verduleria.Data;

public class VerduleriaDbContext : DbContext
{
    public VerduleriaDbContext(DbContextOptions<VerduleriaDbContext> options) : base(options)
    {
    }

    public DbSet<Cajero> Cajeros => Set<Cajero>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Venta> Ventas => Set<Venta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.Property(p => p.PrecioPorKg).HasColumnType("decimal(10,2)");
            entity.Property(p => p.StockKg).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.Property(v => v.CantidadKg).HasColumnType("decimal(10,2)");
            entity.Property(v => v.Total).HasColumnType("decimal(10,2)");

            entity.HasOne(v => v.Cajero)
                  .WithMany(c => c.Ventas)
                  .HasForeignKey(v => v.CajeroId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Producto)
                  .WithMany(p => p.Ventas)
                  .HasForeignKey(v => v.ProductoId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cajero>().HasData(
            new Cajero { Id = 1, Nombre = "Lucía Gómez", Legajo = "CAJ-01", Activo = true },
            new Cajero { Id = 2, Nombre = "Martín Pérez", Legajo = "CAJ-02", Activo = true }
        );

        modelBuilder.Entity<Producto>().HasData(
            new Producto { Id = 1, Nombre = "Pimiento", PrecioPorKg = 2800m, StockKg = 25m },
            new Producto { Id = 2, Nombre = "Tomate", PrecioPorKg = 1900m, StockKg = 40m },
            new Producto { Id = 3, Nombre = "Pera", PrecioPorKg = 1600m, StockKg = 30m },
            new Producto { Id = 4, Nombre = "Cebolla", PrecioPorKg = 1200m, StockKg = 50m },
            new Producto { Id = 5, Nombre = "Banana", PrecioPorKg = 2100m, StockKg = 35m }
        );
    }
}