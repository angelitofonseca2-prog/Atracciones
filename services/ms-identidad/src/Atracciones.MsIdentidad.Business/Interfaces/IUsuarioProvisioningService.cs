namespace Atracciones.MsIdentidad.Business.Interfaces;

public interface IUsuarioProvisioningService
{
    Task<(int usuId, Guid usuGuid)> CrearUsuarioAsync(
        string login,
        string passwordPlain,
        IReadOnlyList<string> roles,
        string creadoPor,
        string ipCreador,
        CancellationToken ct = default);

    Task<bool> EliminarUsuarioAsync(Guid usuGuid, CancellationToken ct = default);
}
