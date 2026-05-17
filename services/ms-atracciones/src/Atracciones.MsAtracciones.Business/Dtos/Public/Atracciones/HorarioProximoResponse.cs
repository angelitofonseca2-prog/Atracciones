namespace Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

public class HorarioProximoResponse
{
    public string HorGuid { get; set; } = string.Empty;
    public string TckGuid { get; set; } = string.Empty;
    public string TicketTitulo { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string? FechaFin { get; set; }
    public string HoraInicio { get; set; } = string.Empty;
    public string? HoraFin { get; set; }
    public int Cupos { get; set; }
    public bool Disponible { get; set; }
}
