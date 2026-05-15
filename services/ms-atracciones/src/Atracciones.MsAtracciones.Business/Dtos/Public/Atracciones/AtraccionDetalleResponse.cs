namespace Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

public class AtraccionDetalleResponse : AtraccionListadoResponse
{
    public string Descripcion { get; set; } = string.Empty;
    public IList<string> Imagenes { get; set; } = new List<string>();
    public IList<string> Incluye { get; set; } = new List<string>();
    public IList<string> NoIncluye { get; set; } = new List<string>();
    public string? PuntoEncuentro { get; set; }
    public bool IncluyeTransporte { get; set; }
    public bool IncluyeAcompaniante { get; set; }
    public IList<TicketDisponibleResponse> Tickets { get; set; } = new List<TicketDisponibleResponse>();
    public IList<HorarioProximoResponse> HorariosProximos { get; set; } = new List<HorarioProximoResponse>();
}
