using Facturacion.Domain.Entities;

namespace Facturacion.Application.Interfaces.Repositories;

public interface IFacturaRepository
{
    Task<List<Factura>> GetAllAsync(Guid? envioId);
    Task<Factura?> GetByIdAsync(Guid id);
    Task<bool> ExistsByEnvioIdAsync(Guid envioId);
    void Add(Factura factura);
}
