namespace Atracciones.MsCatalogos.DataAccess.Entities;

public sealed class ImagenEntity
{
    public Guid ImgGuid { get; set; }
    public string ImgUrl { get; set; } = string.Empty;
    public string? ImgDescripcion { get; set; }
    public DateTime ImgFechaIngreso { get; set; }
    public string ImgUsuarioIngreso { get; set; } = string.Empty;
    public string ImgIpIngreso { get; set; } = string.Empty;
    public DateTime? ImgFechaMod { get; set; }
    public string? ImgUsuarioMod { get; set; }
    public string? ImgIpMod { get; set; }
    public DateTime? ImgFechaEliminacion { get; set; }
    public string? ImgUsuarioEliminacion { get; set; }
    public string? ImgIpEliminacion { get; set; }
    public char ImgEstado { get; set; } = 'A';
}
