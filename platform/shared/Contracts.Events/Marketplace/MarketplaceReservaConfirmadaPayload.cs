using System.Text.Json.Serialization;

namespace Atracciones.Contracts.Events.Marketplace;

public sealed class MarketplaceReservaConfirmadaPayload
{
    [JsonPropertyName("seguimiento_id")]
    public Guid SeguimientoId { get; init; }

    [JsonPropertyName("rev_guid")]
    public Guid RevGuid { get; init; }

    [JsonPropertyName("rev_codigo")]
    public string RevCodigo { get; init; } = string.Empty;

    [JsonPropertyName("cli_guid")]
    public Guid CliGuid { get; init; }

    [JsonPropertyName("at_guid")]
    public Guid AtGuid { get; init; }

    [JsonPropertyName("hor_guid")]
    public Guid HorGuid { get; init; }

    [JsonPropertyName("total")]
    public decimal Total { get; init; }

    [JsonPropertyName("estado")]
    public string Estado { get; init; } = "P";
}
