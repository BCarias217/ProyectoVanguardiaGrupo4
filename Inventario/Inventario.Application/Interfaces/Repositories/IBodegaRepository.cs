using Inventario.Domain.Entities;

namespace Inventario.Application.Interfaces.Repositories;

public interface IBodegaRepository
{
    Task<List<Bodega>> GetAllAsync();
    Task<Bodega?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
}
