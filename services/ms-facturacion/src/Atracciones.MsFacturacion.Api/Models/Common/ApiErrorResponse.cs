namespace Atracciones.MsFacturacion.Api.Models.Common;

public sealed class ApiErrorResponse
{
    public int Status { get; set; }
    public string Error { get; set; } = string.Empty;
    public IList<string> Details { get; set; } = new List<string>();
    public string Timestamp { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}
