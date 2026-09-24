namespace Inventario.Application.Interfaces;

// Permite que StockService haga varios cambios (existencias, movimiento, detalles)
// y los confirme en un solo SaveChanges: eso es lo que da la atomicidad de R6.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
