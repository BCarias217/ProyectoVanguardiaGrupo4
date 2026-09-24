namespace Inventario.Domain.Entities;

public class MovimientoStock
{
    public Guid Id { get; set; }

    // Idempotencia: reintentar el mismo SolicitudId no debe duplicar el movimiento.
    public Guid SolicitudId { get; set; }

    public TipoMovimiento Tipo { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int? BodegaOrigenId { get; set; }
    public Bodega? BodegaOrigen { get; set; }

    public int? BodegaDestinoId { get; set; }
    public Bodega? BodegaDestino { get; set; }

    public int Cantidad { get; set; }
    public DateTime FechaUtc { get; set; }

    public List<DetalleMovimiento> Detalles { get; set; } = new();
}
