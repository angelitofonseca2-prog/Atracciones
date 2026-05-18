namespace Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

/// <summary>Body para POST /atracciones/{guid}/resenias (at_guid va en la ruta).</summary>
public sealed class CrearReseniaBodyRequest
{
    public Guid RevGuid { get; set; }
    public string? Comentario { get; set; }
    public decimal Rating { get; set; }
}
