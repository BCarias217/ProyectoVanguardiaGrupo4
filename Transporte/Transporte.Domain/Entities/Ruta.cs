namespace Transporte.Domain.Entities;

public class Ruta
{
    public int Id { get; set; }
    public int BodegaOrigenId { get; set; }
    public int BodegaDestinoId { get; set; }
    public decimal DistanciaKm { get; set; }
    public bool Activa { get; set; } = true;
}
