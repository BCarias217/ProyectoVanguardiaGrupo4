using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class ExistenciaRepository : IExistenciaRepository
{
    private readonly InventarioDbContext _context;

    public ExistenciaRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<Existencia?> GetTrackedAsync(int bodegaId, int productoId)
        => await _context.Existencias
            .FirstOrDefaultAsync(e => e.BodegaId == bodegaId && e.ProductoId == productoId);

    public async Task<int?> ObtenerCantidadAsync(int bodegaId, int productoId)
    {
        var existencia = await _context.Existencias
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.BodegaId == bodegaId && e.ProductoId == productoId);

        return existencia?.Cantidad;
    }

    public void Add(Existencia existencia) => _context.Existencias.Add(existencia);
}
