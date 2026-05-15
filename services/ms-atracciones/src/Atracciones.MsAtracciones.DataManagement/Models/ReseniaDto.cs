namespace Atracciones.MsAtracciones.DataManagement.Models;

public sealed class ReseniaDto
{
    public Guid RsnGuid { get; init; }
    public Guid AtGuid { get; init; }
    public Guid RevGuid { get; init; }
    public string? Comentario { get; init; }
    public decimal Rating { get; init; }
    public DateTime FechaCreacion { get; init; }
    public char Estado { get; init; }
}
