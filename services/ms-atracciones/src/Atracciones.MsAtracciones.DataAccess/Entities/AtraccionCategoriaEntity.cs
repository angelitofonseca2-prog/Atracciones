namespace Atracciones.MsAtracciones.DataAccess.Entities;

public sealed class AtraccionCategoriaEntity
{
    public Guid AtGuid { get; set; }
    public Guid CatGuid { get; set; }
    public char CaEstado { get; set; } = 'A';
    public DateTime CaFechaIngreso { get; set; }
    public string CaUsuarioIngreso { get; set; } = string.Empty;

    public AtraccionEntity Atraccion { get; set; } = null!;
}
