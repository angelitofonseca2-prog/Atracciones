using Atracciones.MsAtracciones.DataAccess.Context;
using Atracciones.MsAtracciones.DataAccess.Entities;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsAtracciones.DataAccess.Repositories;

public sealed class InventarioOutboxRepository : IOutboxWriter, IOutboxReader, IProcessedEventStore
{
    private readonly InventarioDbContext _db;

    public InventarioOutboxRepository(InventarioDbContext db) => _db = db;

    public async Task EnqueueAsync(string routingKey, string payloadJson, string correlationId, CancellationToken ct = default)
    {
        _db.OutboxEvents.Add(new OutboxEventEntity
        {
            ObGuid = Guid.NewGuid(),
            RoutingKey = routingKey,
            PayloadJson = payloadJson,
            CorrelationId = correlationId,
            CreatedUtc = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default) =>
        await _db.OutboxEvents
            .Where(x => x.PublishedUtc == null)
            .OrderBy(x => x.CreatedUtc)
            .Take(batchSize)
            .Select(x => new OutboxMessage
            {
                ObGuid = x.ObGuid,
                RoutingKey = x.RoutingKey,
                PayloadJson = x.PayloadJson,
                CorrelationId = x.CorrelationId,
                CreatedUtc = x.CreatedUtc,
            })
            .ToListAsync(ct);

    public async Task MarkPublishedAsync(Guid obGuid, CancellationToken ct = default)
    {
        var row = await _db.OutboxEvents.FirstOrDefaultAsync(x => x.ObGuid == obGuid, ct);
        if (row is null) return;
        row.PublishedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

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
