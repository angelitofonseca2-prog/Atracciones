namespace Atracciones.MsOrquestador.DataAccess.Entities;

public sealed class SagaStateEntity
{
    public Guid SagaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime InicioUtc { get; set; }
    public DateTime? FinUtc { get; set; }

    public ICollection<SagaPasoEntity> Pasos { get; set; } = new List<SagaPasoEntity>();
}
