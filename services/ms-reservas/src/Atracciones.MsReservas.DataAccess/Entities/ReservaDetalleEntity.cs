namespace Atracciones.MsReservas.DataAccess.Entities;

public sealed class ReservaDetalleEntity
{
    public Guid RdetGuid { get; set; }
    public Guid RevGuid { get; set; }
    public Guid TckGuid { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnit { get; set; }
    public decimal SubtotalLinea { get; set; }
    public string TipoParticipante { get; set; } = string.Empty;

    public ReservaEntity Reserva { get; set; } = null!;
}
