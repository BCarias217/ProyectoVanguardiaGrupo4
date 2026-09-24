using Transporte.Application.Interfaces;
using Transporte.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Transporte.Infrastructure.Persistence;

public class TransporteDbContext : DbContext, IUnitOfWork
{
    public TransporteDbContext(DbContextOptions<TransporteDbContext> options) : base(options)
    {
    }

    // DbContext.SaveChangesAsync() trae un parámetro opcional (CancellationToken),
    // así que no satisface IUnitOfWork.SaveChangesAsync() por sí solo: se expone explícito.
    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();

    public DbSet<Ruta> Rutas => Set<Ruta>();
    public DbSet<Envio> Envios => Set<Envio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ruta>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.DistanciaKm).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Envio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DistanciaKmAplicada).HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.Ruta)
                  .WithMany()
                  .HasForeignKey(e => e.RutaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
