using Inventario.Application.Interfaces;
using Inventario.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Persistence;

public class InventarioDbContext : DbContext, IUnitOfWork
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options)
    {
    }

    // DbContext.SaveChangesAsync() trae un parámetro opcional (CancellationToken),
    // así que no satisface IUnitOfWork.SaveChangesAsync() por sí solo: se expone explícito.
    public Task<int> SaveChangesAsync() => base.SaveChangesAsync();

    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();
    public DbSet<DetalleMovimiento> DetallesMovimiento => Set<DetalleMovimiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bodega>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Codigo).IsRequired().HasMaxLength(20);
            entity.HasIndex(b => b.Codigo).IsUnique();
            entity.Property(b => b.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Ubicacion).HasMaxLength(200);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Sku).IsRequired().HasMaxLength(30);
            entity.HasIndex(p => p.Sku).IsUnique();
            entity.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Unidad).HasMaxLength(20);
        });

        modelBuilder.Entity<Existencia>(entity =>
        {
            entity.HasKey(e => new { e.BodegaId, e.ProductoId });

            entity.HasOne(e => e.Bodega)
                  .WithMany()
                  .HasForeignKey(e => e.BodegaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Producto)
                  .WithMany()
                  .HasForeignKey(e => e.ProductoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // R1 - último resguardo: el stock nunca queda negativo, ni ante condiciones de carrera.
            entity.ToTable(t => t.HasCheckConstraint("CK_Existencia_Cantidad_NoNegativa", "\"Cantidad\" >= 0"));
        });

        modelBuilder.Entity<MovimientoStock>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.SolicitudId).IsUnique();

            entity.HasOne(m => m.Producto)
                  .WithMany()
                  .HasForeignKey(m => m.ProductoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.BodegaOrigen)
                  .WithMany()
                  .HasForeignKey(m => m.BodegaOrigenId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.BodegaDestino)
                  .WithMany()
                  .HasForeignKey(m => m.BodegaDestinoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(m => m.Detalles)
                  .WithOne(d => d.MovimientoStock)
                  .HasForeignKey(d => d.MovimientoStockId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetalleMovimiento>(entity =>
        {
            entity.HasKey(d => d.Id);

            entity.HasOne(d => d.Bodega)
                  .WithMany()
                  .HasForeignKey(d => d.BodegaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
