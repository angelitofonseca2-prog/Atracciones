using System.ComponentModel.DataAnnotations;

namespace Atracciones.MsAtracciones.Business.Dtos.Admin.Tickets;

public class ActualizarTicketRequest
{
    [MaxLength(150)] public string? Titulo { get; set; }
    [Range(0, double.MaxValue)] public decimal? Precio { get; set; }
    [Range(0, int.MaxValue)] public int? CuposDisponibles { get; set; }
}
