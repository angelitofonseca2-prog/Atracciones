using System.Text.Json.Serialization;

namespace Atracciones.Contracts.Events.Marketplace;

public sealed class MarketplaceReservaRechazadaPayload
{
    [JsonPropertyName("seguimiento_id")]
    public Guid SeguimientoId { get; init; }

    [JsonPropertyName("rev_guid")]
    public Guid? RevGuid { get; init; }

    [JsonPropertyName("motivo")]
    public string Motivo { get; init; } = string.Empty;
}
