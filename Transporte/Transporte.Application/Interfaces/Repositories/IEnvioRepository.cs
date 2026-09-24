using Transporte.Domain.Entities;

namespace Transporte.Application.Interfaces.Repositories;

public interface IEnvioRepository
{
    Task<Envio?> GetByIdAsync(Guid id);
    void Add(Envio envio);
}
