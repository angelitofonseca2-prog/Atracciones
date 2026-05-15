namespace Atracciones.MsIdentidad.Business.DTOs;

public sealed class UsuarioAutenticadoDto
{
    public int UsuId { get; set; }
    public Guid UsuGuid { get; set; }
    public string Login { get; set; } = string.Empty;
    public int? CliId { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
}
