using System.Text.Json.Serialization;

namespace Atracciones.MsAtracciones.Api.Models.Common;

public sealed class ApiListResponse<T>
{
    public int Status { get; set; } = 200;
    public string Message { get; set; } = "Consulta exitosa";
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public PaginationResponse Pagination { get; set; } = new();

    [JsonPropertyName("filterStats")]
    public FilterStatsResponse FilterStats { get; set; } = new();

    [JsonPropertyName("sorters")]
    public IList<SorterResponse> Sorters { get; set; } = new List<SorterResponse>();

    [JsonPropertyName("defaultSorter")]
    public SorterResponse? DefaultSorter { get; set; }

    [JsonPropertyName("_links")]
    public Dictionary<string, string?> Links { get; set; } = new();
}
