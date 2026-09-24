using Facturacion.Application.Interfaces.Repositories;
using Facturacion.Domain.Entities;
using Facturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infrastructure.Repositories;

public class FacturaRepository : IFacturaRepository
{
    private readonly FacturacionDbContext _context;

    public FacturaRepository(FacturacionDbContext context)
    {
        _context = context;
    }

    public async Task<List<Factura>> GetAllAsync(Guid? envioId)
    {
        var query = _context.Facturas.AsNoTracking().AsQueryable();
        if (envioId is Guid id)
            query = query.Where(f => f.EnvioId == id);

        return await query.OrderByDescending(f => f.FechaUtc).ToListAsync();
    }

    public async Task<Factura?> GetByIdAsync(Guid id)
        => await _context.Facturas.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);

    public async Task<bool> ExistsByEnvioIdAsync(Guid envioId)
        => await _context.Facturas.AnyAsync(f => f.EnvioId == envioId);

    public void Add(Factura factura) => _context.Facturas.Add(factura);
}
