using Facturacion.Application.Interfaces;
using Facturacion.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infrastructure.Persistence;

public class FacturacionDbContext : DbContext, IUnitOfWork
{
    public FacturacionDbContext(DbContextOptions<FacturacionDbContext> options) : base(options)
    {
    }

    public DbSet<Tarifa> Tarifas => Set<Tarifa>();
    public DbSet<Factura> Facturas => Set<Factura>();

    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tarifa>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Version).IsRequired().HasMaxLength(20);
            entity.Property(t => t.CargoBase).HasColumnType("decimal(18,2)");
            entity.Property(t => t.PrecioPorKm).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Moneda).IsRequired().HasMaxLength(3);
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Numero).IsRequired().HasMaxLength(50);

            // R5: respaldo a nivel de BD ante dos solicitudes simultáneas para el mismo envío.
            entity.HasIndex(f => f.EnvioId).IsUnique();

            entity.Property(f => f.DistanciaKmAplicada).HasColumnType("decimal(10,2)");
            entity.Property(f => f.CargoBaseAplicado).HasColumnType("decimal(18,2)");
            entity.Property(f => f.PrecioPorKmAplicado).HasColumnType("decimal(18,2)");
            entity.Property(f => f.Total).HasColumnType("decimal(18,2)");
            entity.Property(f => f.Moneda).IsRequired().HasMaxLength(3);

            entity.HasOne(f => f.Tarifa)
                  .WithMany()
                  .HasForeignKey(f => f.TarifaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
