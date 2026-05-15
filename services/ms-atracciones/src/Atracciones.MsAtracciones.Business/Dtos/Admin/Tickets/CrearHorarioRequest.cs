using System.ComponentModel.DataAnnotations;

namespace Atracciones.MsAtracciones.Business.Dtos.Admin.Tickets;

public class CrearHorarioRequest
{
    [Required] public Guid TckGuid { get; set; }
    [Required] public DateOnly Fecha { get; set; }
    [Required] public TimeOnly HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    [Range(0, int.MaxValue)] public int CuposDisponibles { get; set; }
}
