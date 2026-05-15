namespace Atracciones.MsIdentidad.DataManagement.Models;

public sealed class NuevoUsuarioDto
{
    public string Login { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public string CreadoPor { get; init; } = string.Empty;
    public string IpCreador { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
