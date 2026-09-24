using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class BodegaRepository : IBodegaRepository
{
    private readonly InventarioDbContext _context;

    public BodegaRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<List<Bodega>> GetAllAsync()
        => await _context.Bodegas.AsNoTracking().OrderBy(b => b.Codigo).ToListAsync();

    public async Task<Bodega?> GetByIdAsync(int id)
        => await _context.Bodegas.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

    public async Task<bool> ExistsAsync(int id)
        => await _context.Bodegas.AnyAsync(b => b.Id == id);
}
