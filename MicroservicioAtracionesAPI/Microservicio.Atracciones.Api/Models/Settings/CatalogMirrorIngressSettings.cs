namespace Microservicio.Atracciones.Api.Models.Settings;

public sealed class CatalogMirrorIngressSettings
{
    public const string SectionName = "CatalogMirrorIngress";

    /// <summary>Comparte secreto con ms-catalogos (MonolithCatalogLegacy:SyncApiKey).</summary>
    public string ApiKey { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;
}
