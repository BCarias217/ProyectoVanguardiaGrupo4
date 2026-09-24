using Inventario.Domain.Entities;

namespace Inventario.Application.Interfaces.Repositories;

public interface IProductoRepository
{
    Task<List<Producto>> GetAllAsync();
    Task<Producto?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
}
