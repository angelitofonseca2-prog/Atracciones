using System.Text.Json.Serialization;

namespace Atracciones.MsAtracciones.Api.Models.Common;

public sealed class FilterStatsResponse
{
    [JsonPropertyName("filteredProductCount")]
    public int FilteredProductCount { get; set; }

    [JsonPropertyName("unfilteredProductCount")]
    public int UnfilteredProductCount { get; set; }
}
