using Transporte.Domain.Entities;

namespace Transporte.Domain.Validation;

public class EnvioEstadoValidator : IEnvioEstadoValidator
{
    private static readonly Dictionary<EstadoEnvio, EstadoEnvio> TransicionesValidas = new()
    {
        [EstadoEnvio.Pendiente] = EstadoEnvio.EnTransito,
        [EstadoEnvio.EnTransito] = EstadoEnvio.Entregado
    };

    public string? ValidarTransicion(EstadoEnvio actual, EstadoEnvio nuevo)
    {
        if (actual == nuevo)
            return $"El envío ya está en estado {actual}.";

        if (!TransicionesValidas.TryGetValue(actual, out var siguiente) || siguiente != nuevo)
            return $"No se puede pasar de {actual} a {nuevo}. Orden válido: Pendiente -> EnTransito -> Entregado.";

        return null;
    }
}
