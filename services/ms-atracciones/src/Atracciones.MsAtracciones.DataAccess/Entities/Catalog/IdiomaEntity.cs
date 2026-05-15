namespace Atracciones.MsAtracciones.DataAccess.Entities.Catalog;

public sealed class IdiomaEntity
{
    public Guid IdGuid { get; set; }
    public string IdDescripcion { get; set; } = string.Empty;
    public DateTime IdFechaIngreso { get; set; }
    public string IdUsuarioIngreso { get; set; } = string.Empty;
    public string IdIpIngreso { get; set; } = string.Empty;
    public DateTime? IdFechaMod { get; set; }
    public string? IdUsuarioMod { get; set; }
    public string? IdIpMod { get; set; }
    public DateTime? IdFechaEliminacion { get; set; }
    public string? IdUsuarioEliminacion { get; set; }
    public string? IdIpEliminacion { get; set; }
    public char IdEstado { get; set; } = 'A';
}
