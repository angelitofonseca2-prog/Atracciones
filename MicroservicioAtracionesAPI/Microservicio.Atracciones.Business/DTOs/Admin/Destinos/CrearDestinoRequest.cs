using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Destinos
{
    public class CrearDestinoRequest
    {
        [Required][MaxLength(150)] public string Nombre { get; set; } = string.Empty;
        [Required][MaxLength(100)] public string Pais { get; set; } = string.Empty;
        [MaxLength(500)] public string? ImagenUrl { get; set; }
    }
}
