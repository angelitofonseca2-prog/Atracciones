namespace Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

public class AtraccionListadoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string TipoTagname { get; set; } = string.Empty;
    public string TipoNombre { get; set; } = string.Empty;
    public string? SubtipoTagname { get; set; }
    public string? SubtipoNombre { get; set; }
    public IList<string> Etiquetas { get; set; } = new List<string>();
    public string DescripcionCorta { get; set; } = string.Empty;
    public string? ImagenPrincipal { get; set; }
    public int? DuracionMinutos { get; set; }
    public decimal PrecioDesde { get; set; }
    public string Moneda { get; set; } = "USD";
    public double Calificacion { get; set; }
    public int TotalResenas { get; set; }
    public IList<string> IdiomasDisponibles { get; set; } = new List<string>();
    public DisponibilidadResponse Disponibilidad { get; set; } = new();
    public Dictionary<string, string?> Links { get; set; } = new();
}
