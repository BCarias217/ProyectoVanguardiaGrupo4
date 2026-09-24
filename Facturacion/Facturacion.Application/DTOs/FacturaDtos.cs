namespace Facturacion.Application.DTOs;

public class CrearFacturaRequest
{
    public Guid EnvioId { get; set; }
}

public class FacturaResponse
{
    public Guid Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public Guid EnvioId { get; set; }
    public DateTime FechaUtc { get; set; }
    public decimal DistanciaKmAplicada { get; set; }
    public decimal CargoBaseAplicado { get; set; }
    public decimal PrecioPorKmAplicado { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = string.Empty;
}
