namespace Atracciones.MsAtracciones.Api.Models.Common;

public sealed class PaginationResponse
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
}
