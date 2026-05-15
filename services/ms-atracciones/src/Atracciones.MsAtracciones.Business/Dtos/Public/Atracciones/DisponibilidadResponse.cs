namespace Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

public class DisponibilidadResponse
{
    public bool Disponible { get; set; }
    public bool DisponibleHoy { get; set; }
    public string? ProximaFechaDisponible { get; set; }
    public int? CuposDisponibles { get; set; }
}
