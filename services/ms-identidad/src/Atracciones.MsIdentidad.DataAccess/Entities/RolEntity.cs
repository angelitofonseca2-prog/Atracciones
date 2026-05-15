namespace Atracciones.MsIdentidad.DataAccess.Entities;

public sealed class RolEntity
{
    public int RolId { get; set; }
    public Guid RolGuid { get; set; }
    public string RolDescripcion { get; set; } = string.Empty;
    public char RolEstado { get; set; } = 'A';
    public ICollection<UsuarioRolEntity> UsuarioRoles { get; set; } = new List<UsuarioRolEntity>();
}
