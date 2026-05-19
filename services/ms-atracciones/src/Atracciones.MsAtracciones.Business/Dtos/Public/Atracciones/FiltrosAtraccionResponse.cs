using System.Text.Json.Serialization;

namespace Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

public class OpcionFiltroResponse
{
    public string Name { get; set; } = string.Empty;
    public string Tagname { get; set; } = string.Empty;

    [JsonPropertyName("productCount")]
    public int ProductCount { get; set; }

    public ImagenFiltroResponse? Image { get; set; }

    [JsonPropertyName("childFilterOptions")]
    public IList<OpcionFiltroResponse>? ChildFilterOptions { get; set; }
}

public class ImagenFiltroResponse
{
    public string Url { get; set; } = string.Empty;
}

public class FiltrosAtraccionResponse
{
    [JsonPropertyName("destinationFilters")]
    public IList<OpcionFiltroResponse> DestinationFilters { get; set; } = new List<OpcionFiltroResponse>();

    [JsonPropertyName("typeFilters")]
    public IList<OpcionFiltroResponse> TypeFilters { get; set; } = new List<OpcionFiltroResponse>();

    [JsonPropertyName("labelFilters")]
    public IList<OpcionFiltroResponse> LabelFilters { get; set; } = new List<OpcionFiltroResponse>();

    [JsonPropertyName("minRatingFilter")]
    public IList<OpcionFiltroResponse> MinRatingFilter { get; set; } = new List<OpcionFiltroResponse>();

    [JsonPropertyName("timeOfDayFilters")]
    public IList<OpcionFiltroResponse> TimeOfDayFilters { get; set; } = new List<OpcionFiltroResponse>();

    [JsonPropertyName("supportedLanguageFilters")]
    public IList<OpcionFiltroResponse> SupportedLanguageFilters { get; set; } = new List<OpcionFiltroResponse>();

    [JsonPropertyName("ufiFilters")]
    public IList<OpcionFiltroResponse> UfiFilters { get; set; } = new List<OpcionFiltroResponse>();
}
