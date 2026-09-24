namespace Facturacion.Domain.Entities;

public class Tarifa
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public decimal CargoBase { get; set; }
    public decimal PrecioPorKm { get; set; }
    public string Moneda { get; set; } = "HNL";
    public bool Activa { get; set; } = true;
}
