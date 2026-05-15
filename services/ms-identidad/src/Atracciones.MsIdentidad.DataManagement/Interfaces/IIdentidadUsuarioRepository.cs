using Atracciones.MsIdentidad.DataManagement.Models;

namespace Atracciones.MsIdentidad.DataManagement.Interfaces;

public interface IIdentidadUsuarioRepository
{
    Task<UsuarioAuthSnapshot?> ObtenerActivoPorLoginAsync(string login, CancellationToken ct = default);
    Task<UsuarioAuthSnapshot?> ObtenerActivoPorGuidAsync(Guid usuGuid, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListarRolesPorUsuIdAsync(int usuId, CancellationToken ct = default);
    Task UpsertEspejoMonolithAsync(MonolithUsuarioEspejoDto dto, CancellationToken ct = default);
    Task<(int usuId, Guid usuGuid)> CrearUsuarioConRolesAsync(NuevoUsuarioDto dto, CancellationToken ct = default);
    Task<(IReadOnlyList<UsuarioAdminListItem> Items, int Total)> ListarParaAdminAsync(int page, int limit, CancellationToken ct = default);
    Task<bool> MarcarInactivoPorGuidAsync(Guid usuGuid, CancellationToken ct = default);
}
