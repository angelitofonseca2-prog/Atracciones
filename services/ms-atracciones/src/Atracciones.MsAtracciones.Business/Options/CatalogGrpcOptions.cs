namespace Atracciones.MsAtracciones.Business.Options;

public sealed class CatalogGrpcOptions
{
    public const string SectionName = "CatalogGrpc";

    public string Address { get; set; } = "http://localhost:5301";
}
