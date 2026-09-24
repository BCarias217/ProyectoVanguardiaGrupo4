namespace Facturacion.Application.Interfaces;

public enum ConsultaEnvioEstado
{
    Encontrado,
    NoEncontrado,

    // R4: Transporte cayó o no respondió dentro del timeout corto (ver Avance 1).
    ServicioNoDisponible
}

public record ConsultaEnvioResultado(ConsultaEnvioEstado Estado, string? EstadoEnvio, decimal? DistanciaKmAplicada);

// Facturación nunca toca la base de datos de Transporte: solo consulta su API pública por HTTP.
public interface ITransporteClient
{
    Task<ConsultaEnvioResultado> ObtenerEnvioAsync(Guid envioId);
}
