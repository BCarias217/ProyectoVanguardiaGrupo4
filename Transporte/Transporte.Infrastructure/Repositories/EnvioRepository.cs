using Transporte.Application.Interfaces.Repositories;
using Transporte.Domain.Entities;
using Transporte.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Transporte.Infrastructure.Repositories;

public class EnvioRepository : IEnvioRepository
{
    private readonly TransporteDbContext _context;

    public EnvioRepository(TransporteDbContext context)
    {
        _context = context;
    }

    // Sin AsNoTracking: se necesita rastrear el envío para poder actualizar su
    // estado y versión dentro del mismo SaveChanges.
    public async Task<Envio?> GetByIdAsync(Guid id)
        => await _context.Envios.FirstOrDefaultAsync(e => e.Id == id);

    public void Add(Envio envio) => _context.Envios.Add(envio);
}
