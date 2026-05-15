namespace Microservicio.Atracciones.Api.Models.Settings;

public sealed class IdentidadSyncSettings
{
    public const string SectionName = "Identidad";

    public string BaseUrl { get; set; } = string.Empty;
    public string SyncApiKey { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
