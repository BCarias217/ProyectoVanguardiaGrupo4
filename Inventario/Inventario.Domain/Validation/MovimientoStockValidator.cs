using Inventario.Domain.Entities;

namespace Inventario.Domain.Validation;

public class MovimientoStockValidator : IMovimientoStockValidator
{
    public string? Validar(TipoMovimiento tipo, int productoId, int? bodegaOrigenId, int? bodegaDestinoId, int cantidad)
    {
        if (productoId <= 0)
            return "Debe indicar un producto válido.";

        if (cantidad <= 0)
            return "La cantidad debe ser mayor a cero.";

        switch (tipo)
        {
            case TipoMovimiento.Entrada:
                if (bodegaDestinoId is null or <= 0)
                    return "Una entrada requiere una bodega destino válida.";
                break;

            case TipoMovimiento.Salida:
                if (bodegaOrigenId is null or <= 0)
                    return "Una salida requiere una bodega origen válida.";
                break;

            case TipoMovimiento.Transferencia:
                if (bodegaOrigenId is null or <= 0 || bodegaDestinoId is null or <= 0)
                    return "Una transferencia requiere bodega origen y bodega destino válidas.";
                if (bodegaOrigenId == bodegaDestinoId)
                    return "La bodega origen y la bodega destino no pueden ser la misma.";
                break;

            default:
                return "Tipo de movimiento no reconocido.";
        }

        return null;
    }
}
