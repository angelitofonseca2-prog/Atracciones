namespace Atracciones.MsCatalogos.Business;

public sealed class MonolithCatalogLegacySyncOptions
{
    public const string SectionName = "MonolithCatalogLegacy";

    public string BaseUrl { get; set; } = string.Empty;
    public string SyncApiKey { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}
