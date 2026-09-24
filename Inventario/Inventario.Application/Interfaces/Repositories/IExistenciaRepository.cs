using Inventario.Domain.Entities;

namespace Inventario.Application.Interfaces.Repositories;

public interface IExistenciaRepository
{
    // Sin AsNoTracking: la entidad queda rastreada para poder modificar Cantidad
    // y que EF Core la incluya en el mismo SaveChanges que el movimiento.
    Task<Existencia?> GetTrackedAsync(int bodegaId, int productoId);

    // Lectura simple para el endpoint de consulta de stock (no participa en escritura).
    Task<int?> ObtenerCantidadAsync(int bodegaId, int productoId);

    void Add(Existencia existencia);
}
