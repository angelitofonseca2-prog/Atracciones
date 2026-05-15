using Atracciones.MsAtracciones.DataAccess.Context;
using Atracciones.MsAtracciones.DataAccess.Entities;
using Atracciones.MsAtracciones.DataManagement.Interfaces;
using Atracciones.MsAtracciones.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsAtracciones.DataAccess.Repositories;

public sealed class ReseniaRepository : IReseniaRepository
{
    private readonly InventarioDbContext _db;

    public ReseniaRepository(InventarioDbContext db) => _db = db;

    private static ReseniaDto Map(ReseniaEntity x) => new()
    {
        RsnGuid = x.RsnGuid,
        AtGuid = x.AtGuid,
        RevGuid = x.RevGuid,
        Comentario = x.RsnComentario,
        Rating = x.RsnRating,
        FechaCreacion = x.RsnFechaCreacion,
        Estado = x.RsnEstado,
    };

    public async Task<IReadOnlyList<ReseniaDto>> ListPorAtraccionAsync(Guid atGuid, int page, int pageSize, CancellationToken ct = default)
        => (await _db.Resenias.AsNoTracking()
            .Where(r => r.AtGuid == atGuid && r.RsnEstado == 'A')
            .OrderByDescending(r => r.RsnFechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct))
            .Select(Map).ToList();

    public Task<int> ContarPorAtraccionAsync(Guid atGuid, CancellationToken ct = default)
        => _db.Resenias.AsNoTracking().CountAsync(r => r.AtGuid == atGuid && r.RsnEstado == 'A', ct);

    public async Task<ReseniaDto?> ObtenerPorGuidAsync(Guid rsnGuid, CancellationToken ct = default)
    {
        var x = await _db.Resenias.AsNoTracking().FirstOrDefaultAsync(r => r.RsnGuid == rsnGuid && r.RsnEstado == 'A', ct);
        return x is null ? null : Map(x);
    }

    public Task<bool> ExistePorRevGuidAsync(Guid revGuid, CancellationToken ct = default)
        => _db.Resenias.AsNoTracking().AnyAsync(r => r.RevGuid == revGuid && r.RsnEstado == 'A', ct);

    public async Task<ReseniaDto> CrearAsync(CrearReseniaDto dto, CancellationToken ct = default)
    {
        var entity = new ReseniaEntity
        {
            RsnGuid = Guid.NewGuid(),
            AtGuid = dto.AtGuid,
            RevGuid = dto.RevGuid,
            RsnComentario = dto.Comentario?.Trim(),
            RsnRating = dto.Rating,
            RsnEstado = 'A',
            RsnFechaCreacion = DateTime.UtcNow,
            RsnUsuarioCreacion = dto.UsuarioCreacion,
            RsnIpCreacion = dto.IpCreacion,
        };
        _db.Resenias.Add(entity);
        await _db.SaveChangesAsync(ct);
        return Map(entity);
    }
}
