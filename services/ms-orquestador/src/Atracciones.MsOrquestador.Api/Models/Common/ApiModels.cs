namespace Atracciones.MsOrquestador.Api.Models.Common;

public sealed class ApiErrorResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public IList<string> Details { get; set; } = new List<string>();
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    public string Path { get; set; } = string.Empty;
}

public sealed class ApiItemResponse<T>
{
    public int Status { get; set; } = 200;
    public string Message { get; set; } = "Operación exitosa";
    public T? Data { get; set; }

    public ApiItemResponse() { }

    public ApiItemResponse(T data, int status = 200, string? message = null)
    {
        Data = data;
        Status = status;
        if (message is not null)
            Message = message;
    }
}

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

    public ApiListResponse(IEnumerable<T> data, int totalRows, int page, int limit)
    {
        Data = data;
        Pagination = new PaginationResponse { Total = totalRows, Page = page, Limit = limit };
    }
}
