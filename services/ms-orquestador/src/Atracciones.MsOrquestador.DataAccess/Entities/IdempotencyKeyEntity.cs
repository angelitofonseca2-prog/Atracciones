namespace Atracciones.MsOrquestador.DataAccess.Entities;

public sealed class IdempotencyKeyEntity
{
    public string StorageKey { get; set; } = string.Empty;
    public string ResponseJson { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}
