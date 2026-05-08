using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Reservas
{
    public class ActualizarEstadoReservaRequest
    {
        [Required]
        [RegularExpression("A|I|C", ErrorMessage = "Estado inválido. Valores: A, I, C.")]
        public char NuevoEstado { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}
