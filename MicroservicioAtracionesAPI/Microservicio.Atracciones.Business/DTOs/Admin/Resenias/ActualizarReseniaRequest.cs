using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Resenias
{
    public class ActualizarReseniaRequest
    {
        [Range(1, 5)] public short? Rating { get; set; }
        [MaxLength(1000)] public string? Comentario { get; set; }
        public char? Estado { get; set; }
    }
}
