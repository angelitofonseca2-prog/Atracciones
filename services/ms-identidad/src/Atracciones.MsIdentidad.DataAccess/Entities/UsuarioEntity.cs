namespace Atracciones.MsIdentidad.DataAccess.Entities;

public sealed class UsuarioEntity
{
    public int UsuId { get; set; }
    public Guid UsuGuid { get; set; }
    public string UsuLogin { get; set; } = string.Empty;
    public string UsuPasswordHash { get; set; } = string.Empty;
    public DateTime UsuFechaRegistro { get; set; }
    public string UsuUsuarioRegistro { get; set; } = string.Empty;
    public string UsuIpRegistro { get; set; } = string.Empty;
    public char UsuEstado { get; set; } = 'A';
    /// <summary>Denormalizado desde monolito para el claim cli_id del JWT.</summary>
    public int? CliId { get; set; }
    public ICollection<UsuarioRolEntity> UsuarioRoles { get; set; } = new List<UsuarioRolEntity>();
}
