namespace Transporte.Domain.Entities;

public class Envio
{
    public Guid Id { get; set; }

    public int RutaId { get; set; }
    public Ruta? Ruta { get; set; }

    public EstadoEnvio Estado { get; set; }

    // Fijada al crear el envío: si la ruta cambia después, no afecta envíos ya creados.
    public decimal DistanciaKmAplicada { get; set; }

    public DateTime FechaCreacionUtc { get; set; }
    public DateTime FechaActualizacionUtc { get; set; }
    public DateTime? FechaEntregaUtc { get; set; }

    // Token de versión optimista: el cliente debe enviar la versión que leyó
    // por última vez para poder cambiar el estado (evita transiciones basadas en datos viejos).
    public int Version { get; set; }
}
