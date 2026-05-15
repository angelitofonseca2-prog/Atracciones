namespace Atracciones.MsAtracciones.DataAccess.Entities;

public sealed class TicketEntity
{
    public Guid TckGuid { get; set; }
    public Guid AtGuid { get; set; }
    public string TckTitulo { get; set; } = string.Empty;
    public decimal TckPrecio { get; set; }
    public string TckTipoParticipante { get; set; } = "Adulto";
    public int TckCapacidadMaxima { get; set; }
    public int TckCuposDisponibles { get; set; }

    public DateTime TckFechaIngreso { get; set; }
    public string TckUsuarioIngreso { get; set; } = string.Empty;
    public string TckIpIngreso { get; set; } = string.Empty;
    public DateTime? TckFechaMod { get; set; }
    public string? TckUsuarioMod { get; set; }
    public string? TckIpMod { get; set; }
    public DateTime? TckFechaEliminacion { get; set; }
    public string? TckUsuarioEliminacion { get; set; }
    public string? TckIpEliminacion { get; set; }
    public char TckEstado { get; set; } = 'A';

    public AtraccionEntity Atraccion { get; set; } = null!;
    public ICollection<HorarioEntity> Horarios { get; set; } = new List<HorarioEntity>();
}
