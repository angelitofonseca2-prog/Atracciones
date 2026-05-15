namespace Atracciones.MsAtracciones.DataAccess.Entities;

public sealed class HorarioEntity
{
    public Guid HorGuid { get; set; }
    public Guid TckGuid { get; set; }
    public DateOnly HorFecha { get; set; }
    public TimeOnly HorHoraInicio { get; set; }
    public TimeOnly? HorHoraFin { get; set; }
    public int HorCuposDisponibles { get; set; }

    public DateTime HorFechaIngreso { get; set; }
    public string HorUsuarioIngreso { get; set; } = string.Empty;
    public string HorIpIngreso { get; set; } = string.Empty;
    public DateTime? HorFechaMod { get; set; }
    public string? HorUsuarioMod { get; set; }
    public string? HorIpMod { get; set; }
    public DateTime? HorFechaEliminacion { get; set; }
    public string? HorUsuarioEliminacion { get; set; }
    public string? HorIpEliminacion { get; set; }
    public char HorEstado { get; set; } = 'A';

    public TicketEntity Ticket { get; set; } = null!;
}
