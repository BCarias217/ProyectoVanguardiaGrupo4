namespace Facturacion.Domain.Entities;

public class Factura
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;

    // Referencia lógica al servicio Transporte (sin FK física: son bases de datos separadas).
    public Guid EnvioId { get; set; }

    public int TarifaId { get; set; }
    public Tarifa? Tarifa { get; set; }

    public DateTime FechaUtc { get; set; }
    public decimal DistanciaKmAplicada { get; set; }
    public decimal CargoBaseAplicado { get; set; }
    public decimal PrecioPorKmAplicado { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "HNL";
}
