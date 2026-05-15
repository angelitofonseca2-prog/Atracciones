namespace Atracciones.MsAtracciones.Business.Dtos.Admin.Tickets;

public class TicketResponse
{
    public string TckGuid { get; set; } = string.Empty;
    public string AtraccionGuid { get; set; } = string.Empty;
    public string AtraccionNombre { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string TipoParticipante { get; set; } = string.Empty;
    public int CapacidadMaxima { get; set; }
    public int CuposDisponibles { get; set; }
    public char Estado { get; set; }
    public DateTime FechaIngreso { get; set; }
    public IList<HorarioResponse> Horarios { get; set; } = new List<HorarioResponse>();
}
