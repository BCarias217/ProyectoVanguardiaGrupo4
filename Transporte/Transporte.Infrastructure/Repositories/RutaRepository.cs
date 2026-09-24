using Transporte.Application.Interfaces.Repositories;
using Transporte.Domain.Entities;
using Transporte.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Transporte.Infrastructure.Repositories;

public class RutaRepository : IRutaRepository
{
    private readonly TransporteDbContext _context;

    public RutaRepository(TransporteDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ruta>> GetAllAsync()
        => await _context.Rutas.AsNoTracking().OrderBy(r => r.Id).ToListAsync();

    public async Task<Ruta?> GetByIdAsync(int id)
        => await _context.Rutas.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
}
