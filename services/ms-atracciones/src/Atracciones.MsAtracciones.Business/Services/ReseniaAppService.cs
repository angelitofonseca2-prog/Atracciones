using Atracciones.MsAtracciones.Business.Exceptions;
using Atracciones.MsAtracciones.DataManagement.Interfaces;
using Atracciones.MsAtracciones.DataManagement.Models;

namespace Atracciones.MsAtracciones.Business.Services;

public sealed class ReseniaListadoResponse
{
    public IReadOnlyList<ReseniaItemResponse> Items { get; init; } = [];
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}

public sealed class ReseniaItemResponse
{
    public Guid RsnGuid { get; init; }
    public Guid AtGuid { get; init; }
    public Guid RevGuid { get; init; }
    public string? Comentario { get; init; }
    public decimal Rating { get; init; }
    public DateTime FechaCreacion { get; init; }
}

public sealed class CrearReseniaRequest
{
    public Guid AtGuid { get; init; }
    public Guid RevGuid { get; init; }
    public string? Comentario { get; init; }
    public decimal Rating { get; init; }
}

public interface IReseniaAppService
{
    Task<ReseniaListadoResponse> ListarPorAtraccionAsync(Guid atGuid, int page, int pageSize, CancellationToken ct = default);
    Task<ReseniaItemResponse> CrearAsync(CrearReseniaRequest req, string usuario, string ip, CancellationToken ct = default);
}

public sealed class ReseniaAppService : IReseniaAppService
{
    private readonly IReseniaRepository _repo;
    private readonly IInventarioRepository _inventario;

    public ReseniaAppService(IReseniaRepository repo, IInventarioRepository inventario)
    {
        _repo = repo;
        _inventario = inventario;
    }

    public async Task<ReseniaListadoResponse> ListarPorAtraccionAsync(Guid atGuid, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var items = await _repo.ListPorAtraccionAsync(atGuid, page, pageSize, ct);
        var total = await _repo.ContarPorAtraccionAsync(atGuid, ct);

        return new ReseniaListadoResponse
        {
            Items = items.Select(Map).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ReseniaItemResponse> CrearAsync(CrearReseniaRequest req, string usuario, string ip, CancellationToken ct = default)
    {
        if (req.Rating < 1 || req.Rating > 5)
            throw new ValidationException(["El rating debe estar entre 1 y 5."]);

        var atraccionExiste = await _inventario.ExisteAtraccionActivaAsync(req.AtGuid, ct);
        if (!atraccionExiste)
            throw new NotFoundException("Atraccion", req.AtGuid);

        if (await _repo.ExistePorRevGuidAsync(req.RevGuid, ct))
            throw new ConflictException("Ya existe una reseña para esta reserva.");

        var dto = await _repo.CrearAsync(new CrearReseniaDto
        {
            AtGuid = req.AtGuid,
            RevGuid = req.RevGuid,
            Comentario = req.Comentario,
            Rating = req.Rating,
            UsuarioCreacion = usuario,
            IpCreacion = ip,
        }, ct);

        return Map(dto);
    }

    private static ReseniaItemResponse Map(ReseniaDto d) => new()
    {
        RsnGuid = d.RsnGuid,
        AtGuid = d.AtGuid,
        RevGuid = d.RevGuid,
        Comentario = d.Comentario,
        Rating = d.Rating,
        FechaCreacion = d.FechaCreacion,
    };
}
