using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Clientes
{
    public class ActualizarClienteRequest
    {
        [MaxLength(150)][EmailAddress] public string? Correo { get; set; }
        [MaxLength(20)] public string? Telefono { get; set; }
        [MaxLength(300)] public string? Direccion { get; set; }
        [MaxLength(200)] public string? RazonSocial { get; set; }
        [MaxLength(100)] public string? Nombres { get; set; }
        [MaxLength(100)] public string? Apellidos { get; set; }
    }
}
