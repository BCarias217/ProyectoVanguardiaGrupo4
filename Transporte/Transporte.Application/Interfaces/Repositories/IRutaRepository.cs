using Transporte.Domain.Entities;

namespace Transporte.Application.Interfaces.Repositories;

public interface IRutaRepository
{
    Task<List<Ruta>> GetAllAsync();
    Task<Ruta?> GetByIdAsync(int id);
}
