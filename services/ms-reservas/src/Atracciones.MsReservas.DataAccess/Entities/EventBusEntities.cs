namespace Atracciones.MsReservas.DataAccess.Entities;

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

public sealed class MarketplaceReservaSeguimientoEntity
{
    public Guid SeguimientoId { get; set; }
    public Guid? RevGuid { get; set; }
    public string Estado { get; set; } = "EN_PROCESO";
    public string? RevCodigo { get; set; }
    public string? MotivoRechazo { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
