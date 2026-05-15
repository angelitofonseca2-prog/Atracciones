namespace Atracciones.MsIdentidad.DataManagement.Models;

/// <summary>
/// Copia de credenciales desde el monolito tras crear/actualizar usuario allí.
/// </summary>
public sealed class MonolithUsuarioEspejoDto
{
    public int UsuId { get; init; }
    public Guid UsuGuid { get; init; }
    public string Login { get; init; } = string.Empty;
    public string PasswordHash { get; init; } = string.Empty;
    public int? CliId { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
