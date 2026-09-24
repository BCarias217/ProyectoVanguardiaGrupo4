using Inventario.Domain.Entities;

namespace Inventario.Application.DTOs;

public class CrearMovimientoStockRequest
{
    public Guid SolicitudId { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public int ProductoId { get; set; }
    public int? BodegaOrigenId { get; set; }
    public int? BodegaDestinoId { get; set; }
    public int Cantidad { get; set; }
}

public record SaldoActualizado(int BodegaId, int ProductoId, int Cantidad);

public record DetalleMovimientoResponse(int BodegaId, SentidoMovimiento Sentido, int Cantidad);

public class MovimientoStockResponse
{
    public Guid Id { get; set; }
    public Guid SolicitudId { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public int ProductoId { get; set; }
    public int? BodegaOrigenId { get; set; }
    public int? BodegaDestinoId { get; set; }
    public int Cantidad { get; set; }
    public DateTime FechaUtc { get; set; }
    public List<DetalleMovimientoResponse> Detalles { get; set; } = new();
    public List<SaldoActualizado> SaldosActualizados { get; set; } = new();
}

public record ExistenciaResponse(int BodegaId, int ProductoId, int Cantidad);
