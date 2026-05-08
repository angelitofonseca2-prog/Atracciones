using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Destinos
{
    public class ActualizarDestinoRequest
    {
        [MaxLength(150)] public string? Nombre { get; set; }
        [MaxLength(100)] public string? Pais { get; set; }
        [MaxLength(500)] public string? ImagenUrl { get; set; }
    }
}
