using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Clientes
{
    public class CrearClienteRequest
    {
        [Required]
        [MaxLength(20)]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string NumeroIdentificacion { get; set; } = string.Empty;

        [MaxLength(100)] public string? Nombres { get; set; }
        [MaxLength(100)] public string? Apellidos { get; set; }
        [MaxLength(200)] public string? RazonSocial { get; set; }

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [MaxLength(20)] public string? Telefono { get; set; }
        [MaxLength(300)] public string? Direccion { get; set; }

        // Credenciales del usuario de acceso
        [Required][MaxLength(100)] public string Login { get; set; } = string.Empty;
        [Required][MaxLength(256)] public string Password { get; set; } = string.Empty;
    }
}
