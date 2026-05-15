namespace Atracciones.MsFacturacion.Api.Models.Common;

public sealed class PaginationResponse
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}

public sealed class ApiListResponse<T>
{
    public int Status { get; set; } = 200;
    public string Message { get; set; } = "Consulta exitosa";
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public PaginationResponse Pagination { get; set; } = new();

    public ApiListResponse() { }

    public ApiListResponse(IEnumerable<T> data, int totalRows, int page, int limit)
    {
        Data = data;
        Pagination = new PaginationResponse { Total = totalRows, Page = page, Limit = limit };
    }
}
