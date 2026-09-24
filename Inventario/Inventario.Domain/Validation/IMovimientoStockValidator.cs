using Inventario.Domain.Entities;

namespace Inventario.Domain.Validation;

public interface IMovimientoStockValidator
{
    // Reglas puras de forma (no consultan la base de datos).
    string? Validar(TipoMovimiento tipo, int productoId, int? bodegaOrigenId, int? bodegaDestinoId, int cantidad);
}
