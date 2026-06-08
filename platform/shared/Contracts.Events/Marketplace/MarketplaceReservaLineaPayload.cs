using System.Text.Json.Serialization;

namespace Atracciones.Contracts.Events.Marketplace;

public sealed class MarketplaceReservaLineaPayload
{
    [JsonPropertyName("tck_guid")]
    public Guid TckGuid { get; init; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; init; }
}
