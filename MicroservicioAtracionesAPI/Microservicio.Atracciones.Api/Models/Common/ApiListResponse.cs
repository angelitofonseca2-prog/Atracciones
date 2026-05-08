namespace Microservicio.Atracciones.Api.Models.Common
{
    public class ApiListResponse<T>
    {
        public int Status { get; set; } = 200;
        public string Message { get; set; } = "Consulta exitosa";
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public PaginationResponse Pagination { get; set; } = new();
        public FilterStatsResponse FilterStats { get; set; } = new();
        public IList<SorterResponse> Sorters { get; set; } = new List<SorterResponse>();
        public SorterResponse? DefaultSorter { get; set; }
        public Dictionary<string, string?> Links { get; set; } = new();

        public ApiListResponse() { }

        public ApiListResponse(IEnumerable<T> data, int totalRows, int page, int limit)
        {
            Data = data;
            Pagination = new PaginationResponse
            {
                Total = totalRows,
                Page = page,
                Limit = limit
            };
        }
    }
}
