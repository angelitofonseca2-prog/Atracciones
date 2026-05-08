using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El login es obligatorio.")]
        [MaxLength(100)]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;
    }
}
