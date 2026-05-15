namespace Atracciones.MsFacturacion.Api.Models;

public sealed class FacturaResponse
{
    public string FacGuid { get; set; } = string.Empty;
    public string FacNumero { get; set; } = string.Empty;
    public string RevCodigo { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "USD";
    public DateTime FechaEmision { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string NombreReceptor { get; set; } = string.Empty;
    public string CorreoReceptor { get; set; } = string.Empty;
}
