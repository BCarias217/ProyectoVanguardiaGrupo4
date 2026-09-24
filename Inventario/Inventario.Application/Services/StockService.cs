using Inventario.Application.Common;
using Inventario.Application.DTOs;
using Inventario.Application.Interfaces;
using Inventario.Application.Interfaces.Repositories;
using Inventario.Domain.Entities;
using Inventario.Domain.Validation;

namespace Inventario.Application.Services;

public class StockService
{
    private readonly IBodegaRepository _bodegaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IExistenciaRepository _existenciaRepository;
    private readonly IMovimientoStockRepository _movimientoRepository;
    private readonly IMovimientoStockValidator _validator;
    private readonly IUnitOfWork _unitOfWork;

    public StockService(
        IBodegaRepository bodegaRepository,
        IProductoRepository productoRepository,
        IExistenciaRepository existenciaRepository,
        IMovimientoStockRepository movimientoRepository,
        IMovimientoStockValidator validator,
        IUnitOfWork unitOfWork)
    {
        _bodegaRepository = bodegaRepository;
        _productoRepository = productoRepository;
        _existenciaRepository = existenciaRepository;
        _movimientoRepository = movimientoRepository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<(EstadoResultado Estado, string? Mensaje, ExistenciaResponse? Existencia)> ConsultarStockAsync(int bodegaId, int productoId)
    {
        if (!await _bodegaRepository.ExistsAsync(bodegaId))
            return (EstadoResultado.NoEncontrado, "No se encontró la bodega indicada.", null);

        if (!await _productoRepository.ExistsAsync(productoId))
            return (EstadoResultado.NoEncontrado, "No se encontró el producto indicado.", null);

        var cantidad = await _existenciaRepository.ObtenerCantidadAsync(bodegaId, productoId) ?? 0;
        return (EstadoResultado.Exito, null, new ExistenciaResponse(bodegaId, productoId, cantidad));
    }

    public async Task<(EstadoResultado Estado, string? Mensaje, MovimientoStockResponse? Movimiento)> RegistrarMovimientoAsync(CrearMovimientoStockRequest request)
    {
        // Idempotencia: si ya existe un movimiento con este SolicitudId, se devuelve
        // el mismo resultado en vez de duplicar el efecto (200, no 201).
        var existente = await _movimientoRepository.GetBySolicitudIdAsync(request.SolicitudId);
        if (existente is not null)
            return (EstadoResultado.Exito, null, MapearRespuesta(existente, saldos: new List<SaldoActualizado>()));

        var errorFormato = _validator.Validar(request.Tipo, request.ProductoId, request.BodegaOrigenId, request.BodegaDestinoId, request.Cantidad);
        if (errorFormato is not null)
            return (EstadoResultado.Invalido, errorFormato, null);

        if (!await _productoRepository.ExistsAsync(request.ProductoId))
            return (EstadoResultado.NoEncontrado, "El producto indicado no existe.", null);

        if (request.BodegaOrigenId is int origenId && !await _bodegaRepository.ExistsAsync(origenId))
            return (EstadoResultado.NoEncontrado, "La bodega origen indicada no existe.", null);

        if (request.BodegaDestinoId is int destinoId && !await _bodegaRepository.ExistsAsync(destinoId))
            return (EstadoResultado.NoEncontrado, "La bodega destino indicada no existe.", null);

        var movimiento = new MovimientoStock
        {
            Id = Guid.NewGuid(),
            SolicitudId = request.SolicitudId,
            Tipo = request.Tipo,
            ProductoId = request.ProductoId,
            BodegaOrigenId = request.BodegaOrigenId,
            BodegaDestinoId = request.BodegaDestinoId,
            Cantidad = request.Cantidad,
            FechaUtc = DateTime.UtcNow
        };

        var saldos = new List<SaldoActualizado>();

        // R1: el stock nunca queda negativo — se valida y descuenta dentro de la
        // misma unidad de trabajo que el resto del movimiento (ver R6 más abajo).
        if (request.Tipo is TipoMovimiento.Salida or TipoMovimiento.Transferencia)
        {
            var origen = await ObtenerOCrearExistenciaAsync(request.BodegaOrigenId!.Value, request.ProductoId);
            if (origen.Cantidad < request.Cantidad)
                return (EstadoResultado.Conflicto, $"Saldo insuficiente en la bodega origen: disponible {origen.Cantidad}, solicitado {request.Cantidad}.", null);

            origen.Cantidad -= request.Cantidad;
            movimiento.Detalles.Add(new DetalleMovimiento { BodegaId = origen.BodegaId, Sentido = SentidoMovimiento.Salida, Cantidad = request.Cantidad });
            saldos.Add(new SaldoActualizado(origen.BodegaId, origen.ProductoId, origen.Cantidad));
        }

        // R6: una transferencia registra salida y entrada (dos saldos) en una sola transacción local.
        if (request.Tipo is TipoMovimiento.Entrada or TipoMovimiento.Transferencia)
        {
            var destino = await ObtenerOCrearExistenciaAsync(request.BodegaDestinoId!.Value, request.ProductoId);
            destino.Cantidad += request.Cantidad;
            movimiento.Detalles.Add(new DetalleMovimiento { BodegaId = destino.BodegaId, Sentido = SentidoMovimiento.Entrada, Cantidad = request.Cantidad });
            saldos.Add(new SaldoActualizado(destino.BodegaId, destino.ProductoId, destino.Cantidad));
        }

        _movimientoRepository.Add(movimiento);

        // Un solo SaveChanges: existencias + movimiento + detalles se confirman
        // (o fallan) juntos. La restricción Cantidad >= 0 en la BD es el último resguardo.
        await _unitOfWork.SaveChangesAsync();

        return (EstadoResultado.Creado, null, MapearRespuesta(movimiento, saldos));
    }

    public async Task<(EstadoResultado Estado, string? Mensaje, MovimientoStockResponse? Movimiento)> ObtenerPorIdAsync(Guid id)
    {
        var movimiento = await _movimientoRepository.GetByIdAsync(id);
        if (movimiento is null)
            return (EstadoResultado.NoEncontrado, "No se encontró el movimiento solicitado.", null);

        return (EstadoResultado.Exito, null, MapearRespuesta(movimiento, saldos: new List<SaldoActualizado>()));
    }

    private async Task<Existencia> ObtenerOCrearExistenciaAsync(int bodegaId, int productoId)
    {
        var existencia = await _existenciaRepository.GetTrackedAsync(bodegaId, productoId);
        if (existencia is not null)
            return existencia;

        existencia = new Existencia { BodegaId = bodegaId, ProductoId = productoId, Cantidad = 0 };
        _existenciaRepository.Add(existencia);
        return existencia;
    }

    private static MovimientoStockResponse MapearRespuesta(MovimientoStock movimiento, List<SaldoActualizado> saldos) => new()
    {
        Id = movimiento.Id,
        SolicitudId = movimiento.SolicitudId,
        Tipo = movimiento.Tipo,
        ProductoId = movimiento.ProductoId,
        BodegaOrigenId = movimiento.BodegaOrigenId,
        BodegaDestinoId = movimiento.BodegaDestinoId,
        Cantidad = movimiento.Cantidad,
        FechaUtc = movimiento.FechaUtc,
        Detalles = movimiento.Detalles
            .Select(d => new DetalleMovimientoResponse(d.BodegaId, d.Sentido, d.Cantidad))
            .ToList(),
        SaldosActualizados = saldos
    };
}
