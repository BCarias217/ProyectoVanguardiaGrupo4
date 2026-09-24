using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class MovimientoStockRepository : IMovimientoStockRepository
{
    private readonly InventarioDbContext _context;

    public MovimientoStockRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<MovimientoStock?> GetByIdAsync(Guid id)
        => await _context.MovimientosStock
            .Include(m => m.Detalles)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<MovimientoStock?> GetBySolicitudIdAsync(Guid solicitudId)
        => await _context.MovimientosStock
            .Include(m => m.Detalles)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.SolicitudId == solicitudId);

    public void Add(MovimientoStock movimiento) => _context.MovimientosStock.Add(movimiento);
}
