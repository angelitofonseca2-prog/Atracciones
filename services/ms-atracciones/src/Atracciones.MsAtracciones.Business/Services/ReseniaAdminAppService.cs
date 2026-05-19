using Atracciones.MsAtracciones.Business.Dtos.Admin.Resenias;
using Atracciones.MsAtracciones.Business.Exceptions;
using Atracciones.MsAtracciones.DataManagement.Interfaces;

namespace Atracciones.MsAtracciones.Business.Services;

public interface IReseniaAdminAppService
{
    Task<ReseniaAdminResponse> ObtenerPorGuidAsync(Guid rsnGuid, CancellationToken ct = default);
    Task<IReadOnlyList<ReseniaAdminResponse>> ListarPorAtraccionAsync(Guid atGuid, CancellationToken ct = default);
    Task<ReseniaAdminResponse> ActualizarAsync(
        Guid rsnGuid,
        ActualizarReseniaAdminRequest request,
        string usuarioAccion,
        string ip,
        CancellationToken ct = default);
    Task EliminarAsync(Guid rsnGuid, string usuarioAccion, string ip, CancellationToken ct = default);
}

public sealed class ReseniaAdminAppService : IReseniaAdminAppService
{
    private readonly IReseniaRepository _repo;
    private readonly IInventarioRepository _inventario;

    public ReseniaAdminAppService(IReseniaRepository repo, IInventarioRepository inventario)
    {
        _repo = repo;
        _inventario = inventario;
    }

    public async Task<ReseniaAdminResponse> ObtenerPorGuidAsync(Guid rsnGuid, CancellationToken ct = default)
    {
        var dto = await _repo.ObtenerAdminPorGuidAsync(rsnGuid, ct)
            ?? throw new NotFoundException("Reseña", rsnGuid);
        return await MapAsync(dto, ct);
    }

    public async Task<IReadOnlyList<ReseniaAdminResponse>> ListarPorAtraccionAsync(Guid atGuid, CancellationToken ct = default)
    {
        _ = await _inventario.ObtenerAtraccionAdminAsync(atGuid, ct)
            ?? throw new NotFoundException("Atraccion", atGuid);

        var items = await _repo.ListAdminPorAtraccionAsync(atGuid, ct);
        var atr = await _inventario.ObtenerAtraccionAdminAsync(atGuid, ct);
        var nombre = atr?.AtNombre ?? string.Empty;

        return items.Select(d => ToResponse(d, nombre)).ToList();
    }

    public async Task<ReseniaAdminResponse> ActualizarAsync(
        Guid rsnGuid,
        ActualizarReseniaAdminRequest request,
        string usuarioAccion,
        string ip,
        CancellationToken ct = default)
    {
        if (request.Rating is not null && (request.Rating < 1 || request.Rating > 5))
            throw new ValidationException(["El rating debe estar entre 1 y 5."]);

        if (request.Comentario?.Length > 1000)
            throw new ValidationException(["El comentario no puede superar 1000 caracteres."]);

        _ = await _repo.ObtenerAdminPorGuidAsync(rsnGuid, ct)
            ?? throw new NotFoundException("Reseña", rsnGuid);

        var updated = await _repo.ActualizarAdminAsync(
            rsnGuid,
            request.Rating,
            request.Comentario,
            request.Estado,
            usuarioAccion,
            ip,
            ct);

        return await MapAsync(updated, ct);
    }

    public async Task EliminarAsync(Guid rsnGuid, string usuarioAccion, string ip, CancellationToken ct = default)
    {
        _ = await _repo.ObtenerAdminPorGuidAsync(rsnGuid, ct)
            ?? throw new NotFoundException("Reseña", rsnGuid);
        await _repo.EliminarLogicoAsync(rsnGuid, usuarioAccion, ip, ct);
    }

    private async Task<ReseniaAdminResponse> MapAsync(DataManagement.Models.ReseniaDto dto, CancellationToken ct)
    {
        var atr = await _inventario.ObtenerAtraccionAdminAsync(dto.AtGuid, ct);
        return ToResponse(dto, atr?.AtNombre ?? string.Empty);
    }

    private static ReseniaAdminResponse ToResponse(DataManagement.Models.ReseniaDto d, string atraccionNombre) => new()
    {
        RsnGuid = d.RsnGuid,
        AtGuid = d.AtGuid,
        AtraccionNombre = atraccionNombre,
        RevGuid = d.RevGuid,
        Rating = d.Rating,
        Comentario = d.Comentario,
        Estado = d.Estado,
        FechaCreacion = d.FechaCreacion,
    };
}
