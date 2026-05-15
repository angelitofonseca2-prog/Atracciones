namespace Microservicio.Atracciones.Business.Interfaces.Integration;

/// <summary>
/// Sincroniza credenciales con ms-identidad (JWKS) y opcionalmente devuelve el JWT emitido allí.
/// </summary>
public interface IIdentidadUsuarioSyncPublisher
{
    /// <summary>
    /// Si identidad está deshabilitada o falla la llamada, devuelve null.
    /// </summary>
    Task<IdentidadTokenResult?> SincronizarYObtenerTokenAsync(
        IdentidadUsuarioEspejo espejo,
        CancellationToken cancellationToken = default);
}

public sealed record IdentidadUsuarioEspejo(
    int UsuId,
    Guid UsuGuid,
    string Login,
    string PasswordHash,
    int? CliId,
    IReadOnlyList<string> Roles);

public sealed record IdentidadTokenResult(
    string Token,
    DateTime Expiracion,
    string Login,
    IReadOnlyList<string> Roles);
