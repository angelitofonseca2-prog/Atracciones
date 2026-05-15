namespace Atracciones.MsAtracciones.DataAccess.Entities;

public sealed class AtraccionImagenEntity
{
    public Guid AtGuid { get; set; }
    public Guid ImgGuid { get; set; }
    public string ImgUrlSnap { get; set; } = string.Empty;
    public int ImaOrden { get; set; }
    public char ImaEstado { get; set; } = 'A';
    public DateTime ImaFechaIngreso { get; set; }
    public string ImaUsuarioIngreso { get; set; } = string.Empty;

    public AtraccionEntity Atraccion { get; set; } = null!;
}
