using Facturacion.Application.Common;
using Facturacion.Application.DTOs;
using Facturacion.Application.Interfaces;
using Facturacion.Application.Interfaces.Repositories;
using Facturacion.Domain.Entities;

namespace Facturacion.Application.Services;

public class FacturaService
{
    private const string EstadoEntregado = "Entregado";

    private readonly IFacturaRepository _facturaRepository;
    private readonly ITarifaRepository _tarifaRepository;
    private readonly ITransporteClient _transporteClient;
    private readonly IUnitOfWork _unitOfWork;

    public FacturaService(
        IFacturaRepository facturaRepository,
        ITarifaRepository tarifaRepository,
        ITransporteClient transporteClient,
        IUnitOfWork unitOfWork)
    {
        _facturaRepository = facturaRepository;
        _tarifaRepository = tarifaRepository;
        _transporteClient = transporteClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<(EstadoResultado Estado, string? Mensaje, FacturaResponse? Factura)> CrearAsync(CrearFacturaRequest request)
    {
        // R5: un envío, una factura. Se valida primero para no golpear a Transporte sin necesidad.
        if (await _facturaRepository.ExistsByEnvioIdAsync(request.EnvioId))
            return (EstadoResultado.Conflicto, "Ese envío ya tiene una factura registrada.", null);

        // R4: se consulta el estado real del envío antes de facturar. Si Transporte no
        // responde a tiempo, no se asume nada — se devuelve 503 y no se crea la factura.
        var consulta = await _transporteClient.ObtenerEnvioAsync(request.EnvioId);

        switch (consulta.Estado)
        {
            case ConsultaEnvioEstado.ServicioNoDisponible:
                return (EstadoResultado.DependenciaNoDisponible, "El servicio de Transporte no respondió a tiempo; no se generó la factura.", null);
            case ConsultaEnvioEstado.NoEncontrado:
                return (EstadoResultado.NoEncontrado, "El envío indicado no existe en Transporte.", null);
        }

        if (consulta.EstadoEnvio != EstadoEntregado)
            return (EstadoResultado.Conflicto, $"El envío está en estado {consulta.EstadoEnvio}; solo se factura un envío Entregado.", null);

        var tarifa = await _tarifaRepository.ObtenerActivaAsync();
        if (tarifa is null)
            return (EstadoResultado.Conflicto, "No hay una tarifa activa configurada.", null);

        var distancia = consulta.DistanciaKmAplicada ?? 0m;
        var total = tarifa.CargoBase + tarifa.PrecioPorKm * distancia;

        var factura = new Factura
        {
            Id = Guid.NewGuid(),
            Numero = GenerarNumero(),
            EnvioId = request.EnvioId,
            TarifaId = tarifa.Id,
            FechaUtc = DateTime.UtcNow,
            DistanciaKmAplicada = distancia,
            CargoBaseAplicado = tarifa.CargoBase,
            PrecioPorKmAplicado = tarifa.PrecioPorKm,
            Total = total,
            Moneda = tarifa.Moneda
        };

        _facturaRepository.Add(factura);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Último resguardo de R5: el índice único en EnvioId atrapa la carrera entre
            // dos solicitudes simultáneas que pasaron el ExistsByEnvioIdAsync casi a la vez.
            if (await _facturaRepository.ExistsByEnvioIdAsync(request.EnvioId))
                return (EstadoResultado.Conflicto, "Ese envío ya tiene una factura registrada.", null);

            throw;
        }

        return (EstadoResultado.Creado, null, MapearRespuesta(factura));
    }

    public async Task<List<FacturaResponse>> ObtenerTodasAsync(Guid? envioId)
    {
        var facturas = await _facturaRepository.GetAllAsync(envioId);
        return facturas.Select(MapearRespuesta).ToList();
    }

    public async Task<(EstadoResultado Estado, FacturaResponse? Factura)> ObtenerPorIdAsync(Guid id)
    {
        var factura = await _facturaRepository.GetByIdAsync(id);
        return factura is null
            ? (EstadoResultado.NoEncontrado, null)
            : (EstadoResultado.Exito, MapearRespuesta(factura));
    }

    private static string GenerarNumero()
        => $"FAC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

    private static FacturaResponse MapearRespuesta(Factura factura) => new()
    {
        Id = factura.Id,
        Numero = factura.Numero,
        EnvioId = factura.EnvioId,
        FechaUtc = factura.FechaUtc,
        DistanciaKmAplicada = factura.DistanciaKmAplicada,
        CargoBaseAplicado = factura.CargoBaseAplicado,
        PrecioPorKmAplicado = factura.PrecioPorKmAplicado,
        Total = factura.Total,
        Moneda = factura.Moneda
    };
}
