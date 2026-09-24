namespace Facturacion.Application.Common;

public enum EstadoResultado
{
    Exito,
    Creado,
    NoEncontrado,
    Invalido,
    Conflicto,

    // R4: si Transporte no responde a tiempo, no se inventa un estado — se devuelve 503.
    DependenciaNoDisponible
}
