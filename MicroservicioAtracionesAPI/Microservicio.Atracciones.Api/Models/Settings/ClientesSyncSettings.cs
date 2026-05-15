namespace Microservicio.Atracciones.Api.Models.Settings;

public sealed class ClientesSyncSettings
{
    public const string SectionName = "Clientes";

    public string BaseUrl { get; set; } = string.Empty;
    public string SyncApiKey { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
