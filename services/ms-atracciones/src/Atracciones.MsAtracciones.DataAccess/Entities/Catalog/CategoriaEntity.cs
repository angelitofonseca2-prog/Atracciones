namespace Atracciones.MsAtracciones.DataAccess.Entities.Catalog;

public sealed class CategoriaEntity
{
    public Guid CatGuid { get; set; }
    public Guid? CatParentGuid { get; set; }
    public string CatNombre { get; set; } = string.Empty;
    public DateTime CatFechaIngreso { get; set; }
    public string CatUsuarioIngreso { get; set; } = string.Empty;
    public string CatIpIngreso { get; set; } = string.Empty;
    public DateTime? CatFechaMod { get; set; }
    public string? CatUsuarioMod { get; set; }
    public string? CatIpMod { get; set; }
    public DateTime? CatFechaEliminacion { get; set; }
    public string? CatUsuarioEliminacion { get; set; }
    public string? CatIpEliminacion { get; set; }
    public char CatEstado { get; set; } = 'A';
}
