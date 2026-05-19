using Atracciones.MsAtracciones.DataManagement.Models;

namespace Atracciones.MsAtracciones.DataManagement.Interfaces;

public interface IReseniaRepository
{
    Task<IReadOnlyList<ReseniaDto>> ListPorAtraccionAsync(Guid atGuid, int page, int pageSize, CancellationToken ct = default);
    Task<int> ContarPorAtraccionAsync(Guid atGuid, CancellationToken ct = default);
    Task<ReseniaDto?> ObtenerPorGuidAsync(Guid rsnGuid, CancellationToken ct = default);
    Task<bool> ExistePorRevGuidAsync(Guid revGuid, CancellationToken ct = default);
    Task<ReseniaDto> CrearAsync(CrearReseniaDto dto, CancellationToken ct = default);

    Task<IReadOnlyList<ReseniaDto>> ListAdminPorAtraccionAsync(Guid atGuid, CancellationToken ct = default);

    Task<ReseniaDto?> ObtenerAdminPorGuidAsync(Guid rsnGuid, CancellationToken ct = default);

    Task<ReseniaDto> ActualizarAdminAsync(
        Guid rsnGuid,
        decimal? rating,
        string? comentario,
        char? estado,
        string usuarioMod,
        string ipMod,
        CancellationToken ct = default);

    Task EliminarLogicoAsync(Guid rsnGuid, string usuarioElim, string ipElim, CancellationToken ct = default);
}

public sealed class CrearReseniaDto
{
    public Guid AtGuid { get; init; }
    public Guid RevGuid { get; init; }
    public string? Comentario { get; init; }
    public decimal Rating { get; init; }
    public string UsuarioCreacion { get; init; } = string.Empty;
    public string IpCreacion { get; init; } = string.Empty;
}
