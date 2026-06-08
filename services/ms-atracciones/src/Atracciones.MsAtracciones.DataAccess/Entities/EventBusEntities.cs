namespace Atracciones.MsAtracciones.DataAccess.Entities;

public sealed class OutboxEventEntity
{
    public Guid ObGuid { get; set; }
    public string RoutingKey { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime? PublishedUtc { get; set; }
}

public sealed class ProcessedEventEntity
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedUtc { get; set; }
}
