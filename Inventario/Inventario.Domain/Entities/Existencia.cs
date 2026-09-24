namespace Inventario.Domain.Entities;

// Clave compuesta (BodegaId, ProductoId) configurada en el DbContext.
public class Existencia
{
    public int BodegaId { get; set; }
    public Bodega? Bodega { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public int Cantidad { get; set; }
}
