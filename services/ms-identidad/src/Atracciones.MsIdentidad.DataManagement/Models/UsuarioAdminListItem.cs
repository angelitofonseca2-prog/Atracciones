namespace Atracciones.MsIdentidad.DataManagement.Models;

public sealed record UsuarioAdminListItem(
    Guid UsuGuid,
    string Login,
    char Estado,
    IReadOnlyList<string> Roles,
    DateTime FechaRegistro);
