using Transporte.Domain.Entities;

namespace Transporte.Application.DTOs;

public class CrearEnvioRequest
{
    public int RutaId { get; set; }
}

public class ActualizarEstadoEnvioRequest
{
    public EstadoEnvio Estado { get; set; }
    public int Version { get; set; }
}

public class EnvioResponse
{
    public Guid Id { get; set; }
    public int RutaId { get; set; }
    public EstadoEnvio Estado { get; set; }
    public decimal DistanciaKmAplicada { get; set; }
    public DateTime FechaCreacionUtc { get; set; }
    public DateTime FechaActualizacionUtc { get; set; }
    public DateTime? FechaEntregaUtc { get; set; }
    public int Version { get; set; }
}
