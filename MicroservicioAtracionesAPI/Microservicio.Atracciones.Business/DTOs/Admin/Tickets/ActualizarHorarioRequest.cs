using System.ComponentModel.DataAnnotations;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Tickets
{
    public class ActualizarHorarioRequest
    {
        public DateOnly? Fecha { get; set; }
        public TimeOnly? HoraInicio { get; set; }
        public TimeOnly? HoraFin { get; set; }

        [Range(0, int.MaxValue)]
        public int? CuposDisponibles { get; set; }

        [RegularExpression("A|I", ErrorMessage = "Estado inválido. Valores: A, I.")]
        public char? Estado { get; set; }
    }
}
