using Atracciones.MsFacturacion.DataAccess.Context;
using Atracciones.MsFacturacion.DataAccess.Entities;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsFacturacion.DataAccess.Repositories;

public sealed class FacturacionProcessedEventRepository : IProcessedEventStore
{
    private readonly BillingDbContext _db;

    public FacturacionProcessedEventRepository(BillingDbContext db) => _db = db;

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
