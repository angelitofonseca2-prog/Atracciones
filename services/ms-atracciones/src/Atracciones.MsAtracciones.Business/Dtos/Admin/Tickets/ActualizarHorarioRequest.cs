using System.ComponentModel.DataAnnotations;

namespace Atracciones.MsAtracciones.Business.Dtos.Admin.Tickets;

public class ActualizarHorarioRequest
{
    public DateOnly? Fecha { get; set; }
    public DateOnly? FechaFin { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    [Range(0, int.MaxValue)] public int? CuposDisponibles { get; set; }
}
