using Atracciones.MsAuditoria.DataAccess.Context;
using Atracciones.MsAuditoria.DataAccess.Entities;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsAuditoria.DataAccess.Repositories;

public sealed class AuditoriaProcessedEventRepository : IProcessedEventStore
{
    private readonly AuditoriaDbContext _db;

    public AuditoriaProcessedEventRepository(AuditoriaDbContext db) => _db = db;

    public async Task<bool> TryMarkProcessedAsync(Guid eventId, string eventType, CancellationToken ct = default)
    {
        if (await _db.ProcessedEvents.AnyAsync(x => x.EventId == eventId, ct))
            return false;

        _db.ProcessedEvents.Add(new ProcessedEventEntity
        {
            EventId = eventId,
            EventType = eventType,
            ProcessedUtc = DateTime.UtcNow,
        });

        try
        {
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}
