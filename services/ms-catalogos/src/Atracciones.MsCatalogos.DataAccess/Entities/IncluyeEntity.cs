namespace Atracciones.MsCatalogos.DataAccess.Entities;

public sealed class IncluyeEntity
{
    public Guid IncGuid { get; set; }
    public string IncDescripcion { get; set; } = string.Empty;
    public char IncEstado { get; set; } = 'A';
}
