namespace Atracciones.MsAtracciones.Api.Models.Common;

public sealed class ApiListResponse<T>
{
    public int Status { get; set; } = 200;
    public string Message { get; set; } = "Consulta exitosa";
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public PaginationResponse Pagination { get; set; } = new();
    public FilterStatsResponse FilterStats { get; set; } = new();
    public IList<SorterResponse> Sorters { get; set; } = new List<SorterResponse>();
    public SorterResponse? DefaultSorter { get; set; }
    public Dictionary<string, string?> Links { get; set; } = new();
}
