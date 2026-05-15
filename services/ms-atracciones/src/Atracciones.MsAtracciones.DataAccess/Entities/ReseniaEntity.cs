namespace Atracciones.MsAtracciones.DataAccess.Entities;

public sealed class ReseniaEntity
{
    public Guid RsnGuid { get; set; }
    public Guid AtGuid { get; set; }
    public Guid RevGuid { get; set; }
    public string? RsnComentario { get; set; }
    public decimal RsnRating { get; set; }

    public DateTime RsnFechaCreacion { get; set; }
    public string RsnUsuarioCreacion { get; set; } = string.Empty;
    public string RsnIpCreacion { get; set; } = string.Empty;
    public DateTime? RsnFechaMod { get; set; }
    public string? RsnUsuarioMod { get; set; }
    public string? RsnIpMod { get; set; }
    public DateTime? RsnFechaEliminacion { get; set; }
    public string? RsnUsuarioEliminacion { get; set; }
    public string? RsnIpEliminacion { get; set; }
    public char RsnEstado { get; set; } = 'A';

    public AtraccionEntity Atraccion { get; set; } = null!;
}
