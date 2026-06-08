namespace Atracciones.MarketplaceGateway.Options;

public sealed class ServicesOptions
{
    public const string SectionName = "Services";

    public string AtraccionesHttp { get; set; } = "http://localhost:5401";
    public string ReservasHttp { get; set; } = "http://localhost:5601";
}
