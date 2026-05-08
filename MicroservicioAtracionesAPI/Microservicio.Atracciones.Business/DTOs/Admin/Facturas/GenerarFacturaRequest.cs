using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Facturas
{
    public class GenerarFacturaRequest
    {
        [Required] public Guid RevGuid { get; set; }
        [Required][MaxLength(100)] public string NombreReceptor { get; set; } = string.Empty;
        [MaxLength(100)] public string? ApellidoReceptor { get; set; }
        [Required][EmailAddress][MaxLength(150)] public string CorreoReceptor { get; set; } = string.Empty;
        [MaxLength(20)] public string? TelefonoReceptor { get; set; }
        [MaxLength(500)] public string? Observacion { get; set; }
    }
}
