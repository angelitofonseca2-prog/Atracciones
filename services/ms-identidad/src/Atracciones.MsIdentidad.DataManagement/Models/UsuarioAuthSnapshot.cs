namespace Atracciones.MsIdentidad.DataManagement.Models;

public sealed class UsuarioAuthSnapshot
{
    public int UsuId { get; init; }
    public Guid UsuGuid { get; init; }
    public string Login { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public int? CliId { get; init; }
    public char Estado { get; init; }
}
