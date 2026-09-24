namespace Inventario.Domain.Entities;

public class Bodega
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
}
