namespace Atracciones.MsFacturacion.DataAccess.Entities;

public sealed class ProcessedEventEntity
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedUtc { get; set; }
}
