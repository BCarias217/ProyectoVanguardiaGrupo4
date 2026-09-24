using Facturacion.Domain.Entities;

namespace Facturacion.Application.Interfaces.Repositories;

public interface ITarifaRepository
{
    Task<Tarifa?> ObtenerActivaAsync();
}
