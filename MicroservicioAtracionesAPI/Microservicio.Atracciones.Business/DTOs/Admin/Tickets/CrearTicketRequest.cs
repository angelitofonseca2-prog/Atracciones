using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Tickets
{
    public class CrearTicketRequest
    {
        [Required] public Guid AtGuid { get; set; }
        [Required][MaxLength(150)] public string Titulo { get; set; } = string.Empty;
        [Range(0, double.MaxValue)] public decimal Precio { get; set; }

        [RegularExpression("Adulto|Niño|Grupo|Estudiante|Senior",
            ErrorMessage = "TipoParticipante inválido.")]
        public string TipoParticipante { get; set; } = "Adulto";

        [Range(1, int.MaxValue)] public int CapacidadMaxima { get; set; }
        [Range(0, int.MaxValue)] public int CuposDisponibles { get; set; }
    }
}
