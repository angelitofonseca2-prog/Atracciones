namespace Atracciones.MsReservas.DataAccess.Entities;

public sealed class ReservaEntity
{
    public Guid RevGuid { get; set; }
    public Guid CliGuid { get; set; }
    public Guid AtGuid { get; set; }
    public Guid HorGuid { get; set; }
    public string RevCodigo { get; set; } = string.Empty;
    public char RevEstado { get; set; } = 'P';
    public decimal RevSubtotal { get; set; }
    public decimal RevValorIva { get; set; }
    public decimal RevTotal { get; set; }
    public string RevMoneda { get; set; } = "USD";
    public string? RevOrigenCanal { get; set; }
    public DateTime RevFechaReservaUtc { get; set; }
    public string RevUsuarioIngreso { get; set; } = string.Empty;
    public string RevIpIngreso { get; set; } = string.Empty;

    public string AtraccionNombreSnap { get; set; } = string.Empty;
    public string HorFechaSnap { get; set; } = string.Empty;
    public string HorHoraInicioSnap { get; set; } = string.Empty;
    public string HorHoraFinSnap { get; set; } = string.Empty;

    public ICollection<ReservaDetalleEntity> Detalle { get; set; } = new List<ReservaDetalleEntity>();
}
