namespace Facturacion.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
