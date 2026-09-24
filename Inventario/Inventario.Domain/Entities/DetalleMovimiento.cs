namespace Inventario.Domain.Entities;

public class DetalleMovimiento
{
    public int Id { get; set; }

    public Guid MovimientoStockId { get; set; }
    public MovimientoStock? MovimientoStock { get; set; }

    public int BodegaId { get; set; }
    public Bodega? Bodega { get; set; }

    public SentidoMovimiento Sentido { get; set; }
    public int Cantidad { get; set; }
}
