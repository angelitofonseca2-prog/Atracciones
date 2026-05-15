namespace Atracciones.MsReservas.Api.Models.Admin;

public sealed class ReservaDetalleAdminResponse
{
    public string TckGuid { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnit { get; set; }
    public decimal SubtotalLinea { get; set; }
    public string TipoParticipante { get; set; } = string.Empty;
}

public sealed class ReservaAdminResponse
{
    public string RevGuid { get; set; } = string.Empty;
    public string RevCodigo { get; set; } = string.Empty;
    public string CliGuid { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string AtraccionNombre { get; set; } = string.Empty;
    public string HorFecha { get; set; } = string.Empty;
    public string HorHoraInicio { get; set; } = string.Empty;
    public decimal RevTotal { get; set; }
    public char RevEstado { get; set; }
    public DateTime FechaReserva { get; set; }
    public IList<ReservaDetalleAdminResponse> Detalle { get; set; } = new List<ReservaDetalleAdminResponse>();
}
