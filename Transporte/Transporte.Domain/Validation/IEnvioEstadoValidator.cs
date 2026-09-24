using Transporte.Domain.Entities;

namespace Transporte.Domain.Validation;

public interface IEnvioEstadoValidator
{
    // Regla pura: Pendiente -> EnTransito -> Entregado, sin saltos, sin retroceder,
    // sin repetir el mismo estado.
    string? ValidarTransicion(EstadoEnvio actual, EstadoEnvio nuevo);
}
