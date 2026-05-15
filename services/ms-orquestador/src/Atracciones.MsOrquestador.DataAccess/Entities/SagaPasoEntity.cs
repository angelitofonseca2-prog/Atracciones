namespace Atracciones.MsOrquestador.DataAccess.Entities;

public sealed class SagaPasoEntity
{
    public long PasoId { get; set; }
    public Guid SagaId { get; set; }
    public string Paso { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? RequestPayload { get; set; }
    public string? ResponsePayload { get; set; }
    public string? Error { get; set; }
    public DateTime FechaUtc { get; set; }

    public SagaStateEntity Saga { get; set; } = null!;
}
