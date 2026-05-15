namespace Atracciones.MsAuditoria.DataAccess.Entities;

/// <summary>Tabla solo inserciones (append-only).</summary>
public sealed class EventoAuditoriaEntity
{
    public Guid EvtGuid { get; set; }
    public string EvtTipo { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public DateTime FechaUtc { get; set; }
}
