namespace Transporte.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
