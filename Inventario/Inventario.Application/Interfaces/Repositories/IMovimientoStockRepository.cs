using Inventario.Domain.Entities;

namespace Inventario.Application.Interfaces.Repositories;

public interface IMovimientoStockRepository
{
    Task<MovimientoStock?> GetByIdAsync(Guid id);
    Task<MovimientoStock?> GetBySolicitudIdAsync(Guid solicitudId);
    void Add(MovimientoStock movimiento);
}
