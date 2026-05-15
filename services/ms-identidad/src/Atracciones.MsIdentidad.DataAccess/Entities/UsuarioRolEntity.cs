namespace Atracciones.MsIdentidad.DataAccess.Entities;

public sealed class UsuarioRolEntity
{
    public int UsuId { get; set; }
    public int RolId { get; set; }
    public char UsuRolEstado { get; set; } = 'A';
    public UsuarioEntity Usuario { get; set; } = null!;
    public RolEntity Rol { get; set; } = null!;
}
