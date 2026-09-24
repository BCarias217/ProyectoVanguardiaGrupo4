using System.Net;
using System.Net.Http.Json;
using Facturacion.Application.Interfaces;

namespace Facturacion.Infrastructure.Clients;

// DTO minimo: solo lo que Facturación necesita del contrato público de Transporte.
internal class EnvioApiResponse
{
    public Guid Id { get; set; }
    public int RutaId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal DistanciaKmAplicada { get; set; }
}

public class TransporteClient : ITransporteClient
{
    private readonly HttpClient _httpClient;

    public TransporteClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ConsultaEnvioResultado> ObtenerEnvioAsync(Guid envioId)
    {
        try
        {
            // El timeout corto (3s, configurado en Program.cs) y la ausencia de reintentos
            // son deliberados: si Transporte está caído o lento, Facturación falla rápido
            // en vez de bloquear la solicitud (R4 / decisión de resiliencia del Avance 1).
            using var response = await _httpClient.GetAsync($"envios/{envioId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new ConsultaEnvioResultado(ConsultaEnvioEstado.NoEncontrado, null, null);

            if (!response.IsSuccessStatusCode)
                return new ConsultaEnvioResultado(ConsultaEnvioEstado.ServicioNoDisponible, null, null);

            var envio = await response.Content.ReadFromJsonAsync<EnvioApiResponse>();
            if (envio is null)
                return new ConsultaEnvioResultado(ConsultaEnvioEstado.ServicioNoDisponible, null, null);

            return new ConsultaEnvioResultado(ConsultaEnvioEstado.Encontrado, envio.Estado, envio.DistanciaKmAplicada);
        }
        catch (Exception) when (true)
        {
            // Timeout (TaskCanceledException), conexión rechazada (HttpRequestException),
            // o cualquier otra falla de red: se trata igual, como dependencia no disponible.
            return new ConsultaEnvioResultado(ConsultaEnvioEstado.ServicioNoDisponible, null, null);
        }
    }
}
