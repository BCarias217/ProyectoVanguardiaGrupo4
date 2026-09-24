using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly InventarioDbContext _context;

    public ProductoRepository(InventarioDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> GetAllAsync()
        => await _context.Productos.AsNoTracking().OrderBy(p => p.Sku).ToListAsync();

    public async Task<Producto?> GetByIdAsync(int id)
        => await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<bool> ExistsAsync(int id)
        => await _context.Productos.AnyAsync(p => p.Id == id);
}
