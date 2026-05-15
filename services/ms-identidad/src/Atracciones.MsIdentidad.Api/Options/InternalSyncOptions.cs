namespace Atracciones.MsIdentidad.Api.Options;

public sealed class InternalSyncOptions
{
    public const string SectionName = "InternalSync";

    /// <summary>Debe coincidir con la cabecera X-Monolith-Sync-Key enviada por el monolito.</summary>
    public string MonolithApiKey { get; set; } = string.Empty;
}
