namespace Atracciones.MsAtracciones.Business.Dtos.Admin.Resenias;

public sealed class ReseniaAdminResponse
{
    public Guid RsnGuid { get; init; }
    public Guid AtGuid { get; init; }
    public string AtraccionNombre { get; init; } = string.Empty;
    public Guid RevGuid { get; init; }
    public decimal Rating { get; init; }
    public string? Comentario { get; init; }
    public char Estado { get; init; }
    public DateTime FechaCreacion { get; init; }
}

public sealed class ActualizarReseniaAdminRequest
{
    public decimal? Rating { get; set; }
    public string? Comentario { get; set; }
    public char? Estado { get; set; }
}
