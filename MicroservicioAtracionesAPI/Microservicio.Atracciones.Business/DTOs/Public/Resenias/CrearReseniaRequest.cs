using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Resenias
{
    public class CrearReseniaRequest
    {
        [Required(ErrorMessage = "El GUID de la reserva es obligatorio.")]
        public Guid RevGuid { get; set; }

        [Range(1, 5, ErrorMessage = "El rating debe estar entre 1 y 5.")]
        public short Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "El comentario no puede superar 1000 caracteres.")]
        public string? Comentario { get; set; }
    }
}
