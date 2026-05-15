namespace Atracciones.MsAtracciones.DataAccess.Entities.Catalog;

public sealed class DestinoEntity
{
    public Guid DesGuid { get; set; }
    public string DesNombre { get; set; } = string.Empty;
    public string DesPais { get; set; } = string.Empty;
    public string? DesImagenUrl { get; set; }
    public DateTime DesFechaIngreso { get; set; }
    public string DesUsuarioIngreso { get; set; } = string.Empty;
    public string DesIpIngreso { get; set; } = string.Empty;
    public DateTime? DesFechaMod { get; set; }
    public string? DesUsuarioMod { get; set; }
    public string? DesIpMod { get; set; }
    public DateTime? DesFechaEliminacion { get; set; }
    public string? DesUsuarioEliminacion { get; set; }
    public string? DesIpEliminacion { get; set; }
    public char DesEstado { get; set; } = 'A';
}
