namespace Atracciones.Platform.BuildingBlocks.EventBus.Outbox;

public sealed class OutboxMessage
{
    public Guid ObGuid { get; init; }
    public string RoutingKey { get; init; } = string.Empty;
    public string PayloadJson { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
    public DateTime CreatedUtc { get; init; }
}

public interface IOutboxWriter
{
    Task EnqueueAsync(string routingKey, string payloadJson, string correlationId, CancellationToken ct = default);
}

public interface IOutboxReader
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct = default);
    Task MarkPublishedAsync(Guid obGuid, CancellationToken ct = default);
}

public interface IProcessedEventStore
{
    Task<bool> TryMarkProcessedAsync(Guid eventId, string eventType, CancellationToken ct = default);
}
