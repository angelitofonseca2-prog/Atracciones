namespace Atracciones.MsAtracciones.DataAccess.Entities.Catalog;

public sealed class IncluyeEntity
{
    public Guid IncGuid { get; set; }
    public string IncDescripcion { get; set; } = string.Empty;
    public char IncEstado { get; set; } = 'A';
}
