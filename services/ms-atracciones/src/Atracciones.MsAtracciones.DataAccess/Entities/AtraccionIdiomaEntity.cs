namespace Atracciones.MsAtracciones.DataAccess.Entities;

public sealed class AtraccionIdiomaEntity
{
    public Guid AtGuid { get; set; }
    public Guid IdGuid { get; set; }
    public string IdDescripcionSnap { get; set; } = string.Empty;
    public char IaEstado { get; set; } = 'A';
    public DateTime IaFechaIngreso { get; set; }
    public string IaUsuarioIngreso { get; set; } = string.Empty;

    public AtraccionEntity Atraccion { get; set; } = null!;
}
