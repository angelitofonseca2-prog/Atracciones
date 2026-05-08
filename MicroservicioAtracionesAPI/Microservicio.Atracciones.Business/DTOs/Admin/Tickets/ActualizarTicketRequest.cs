using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Tickets
{
    public class ActualizarTicketRequest
    {
        [MaxLength(150)] public string? Titulo { get; set; }
        [Range(0, double.MaxValue)] public decimal? Precio { get; set; }
        [Range(0, int.MaxValue)] public int? CuposDisponibles { get; set; }
        public char? Estado { get; set; }
    }
}
