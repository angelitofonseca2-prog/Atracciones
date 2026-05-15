namespace Atracciones.MsAtracciones.DataAccess.Entities;

public sealed class AtraccionIncluyeEntity
{
    public Guid AtGuid { get; set; }
    public Guid IncGuid { get; set; }
    public string IncDescripcionSnap { get; set; } = string.Empty;
    public char AiEstado { get; set; } = 'A';
    public DateTime AiFechaIngreso { get; set; }
    public string AiUsuarioIngreso { get; set; } = string.Empty;

    public AtraccionEntity Atraccion { get; set; } = null!;
}
