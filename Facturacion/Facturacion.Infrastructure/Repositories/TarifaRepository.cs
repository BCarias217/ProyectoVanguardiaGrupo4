using Facturacion.Application.Interfaces.Repositories;
using Facturacion.Domain.Entities;
using Facturacion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Facturacion.Infrastructure.Repositories;

public class TarifaRepository : ITarifaRepository
{
    private readonly FacturacionDbContext _context;

    public TarifaRepository(FacturacionDbContext context)
    {
        _context = context;
    }

    public async Task<Tarifa?> ObtenerActivaAsync()
        => await _context.Tarifas.AsNoTracking()
            .Where(t => t.Activa)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync();
}
