using Transporte.Application.Common;
using Transporte.Application.DTOs;
using Transporte.Application.Interfaces;
using Transporte.Application.Interfaces.Repositories;
using Transporte.Domain.Entities;
using Transporte.Domain.Validation;

namespace Transporte.Application.Services;

public class EnvioService
{
    private readonly IRutaRepository _rutaRepository;
    private readonly IEnvioRepository _envioRepository;
    private readonly IEnvioEstadoValidator _estadoValidator;
    private readonly IUnitOfWork _unitOfWork;

    public EnvioService(
        IRutaRepository rutaRepository,
        IEnvioRepository envioRepository,
        IEnvioEstadoValidator estadoValidator,
        IUnitOfWork unitOfWork)
    {
        _rutaRepository = rutaRepository;
        _envioRepository = envioRepository;
        _estadoValidator = estadoValidator;
        _unitOfWork = unitOfWork;
    }

    public async Task<(EstadoResultado Estado, string? Mensaje, EnvioResponse? Envio)> CrearAsync(CrearEnvioRequest request)
    {
        // R2: un envío no puede crearse sin una ruta válida y activa.
        var ruta = await _rutaRepository.GetByIdAsync(request.RutaId);
        if (ruta is null)
            return (EstadoResultado.NoEncontrado, "La ruta indicada no existe.", null);

        if (!ruta.Activa)
            return (EstadoResultado.Conflicto, "La ruta indicada está inactiva.", null);

        var ahora = DateTime.UtcNow;
        var envio = new Envio
        {
            Id = Guid.NewGuid(),
            RutaId = ruta.Id,
            Estado = EstadoEnvio.Pendiente,
            DistanciaKmAplicada = ruta.DistanciaKm,
            FechaCreacionUtc = ahora,
            FechaActualizacionUtc = ahora,
            Version = 1
        };

        _envioRepository.Add(envio);
        await _unitOfWork.SaveChangesAsync();

        return (EstadoResultado.Creado, null, MapearRespuesta(envio));
    }

    public async Task<(EstadoResultado Estado, string? Mensaje, EnvioResponse? Envio)> ObtenerPorIdAsync(Guid id)
    {
        var envio = await _envioRepository.GetByIdAsync(id);
        if (envio is null)
            return (EstadoResultado.NoEncontrado, "No se encontró el envío solicitado.", null);

        return (EstadoResultado.Exito, null, MapearRespuesta(envio));
    }

    public async Task<(EstadoResultado Estado, string? Mensaje, EnvioResponse? Envio)> ActualizarEstadoAsync(Guid id, ActualizarEstadoEnvioRequest request)
    {
        var envio = await _envioRepository.GetByIdAsync(id);
        if (envio is null)
            return (EstadoResultado.NoEncontrado, "No se encontró el envío a actualizar.", null);

        // R3 (control de versión): rechaza si el cliente trabaja sobre una copia vieja.
        if (request.Version != envio.Version)
            return (EstadoResultado.Conflicto, $"Versión desactualizada: el envío está en la versión {envio.Version}.", null);

        var error = _estadoValidator.ValidarTransicion(envio.Estado, request.Estado);
        if (error is not null)
            return (EstadoResultado.Conflicto, error, null);

        envio.Estado = request.Estado;
        envio.FechaActualizacionUtc = DateTime.UtcNow;
        if (request.Estado == EstadoEnvio.Entregado)
            envio.FechaEntregaUtc = envio.FechaActualizacionUtc;
        envio.Version += 1;

        await _unitOfWork.SaveChangesAsync();

        return (EstadoResultado.Exito, null, MapearRespuesta(envio));
    }

    private static EnvioResponse MapearRespuesta(Envio envio) => new()
    {
        Id = envio.Id,
        RutaId = envio.RutaId,
        Estado = envio.Estado,
        DistanciaKmAplicada = envio.DistanciaKmAplicada,
        FechaCreacionUtc = envio.FechaCreacionUtc,
        FechaActualizacionUtc = envio.FechaActualizacionUtc,
        FechaEntregaUtc = envio.FechaEntregaUtc,
        Version = envio.Version
    };
}
