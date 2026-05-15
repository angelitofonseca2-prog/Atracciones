using Atracciones.MsOrquestador.DataAccess.Context;
using Atracciones.MsOrquestador.DataAccess.Entities;
using Atracciones.MsOrquestador.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsOrquestador.DataAccess.Repositories;

public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly OrquestadorDbContext _db;

    public IdempotencyRepository(OrquestadorDbContext db) => _db = db;

    public async Task<string?> ObtenerRespuestaSiExisteAsync(string idempotencyKey, string route, string bodyHash, CancellationToken ct = default)
    {
        var key = BuildStorageKey(idempotencyKey, route, bodyHash);
        var row = await _db.IdempotencyKeys.AsNoTracking().FirstOrDefaultAsync(x => x.StorageKey == key, ct);
        return row?.ResponseJson;
    }

    public async Task GuardarRespuestaAsync(string idempotencyKey, string route, string bodyHash, string responseJson, CancellationToken ct = default)
    {
        var key = BuildStorageKey(idempotencyKey, route, bodyHash);
        var existing = await _db.IdempotencyKeys.FirstOrDefaultAsync(x => x.StorageKey == key, ct);
        if (existing is not null)
        {
            existing.ResponseJson = responseJson;
            existing.CreatedUtc = DateTime.UtcNow;
        }
        else
        {
            _db.IdempotencyKeys.Add(new IdempotencyKeyEntity
            {
                StorageKey = key,
                ResponseJson = responseJson,
                CreatedUtc = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    private static string BuildStorageKey(string idempotencyKey, string route, string bodyHash) =>
        $"{idempotencyKey.Trim()}|{route.Trim()}|{bodyHash}";
}
