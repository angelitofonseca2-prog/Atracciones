using System.ComponentModel.DataAnnotations;

namespace Microservicio.Atracciones.Business.DTOs.Auth
{
    public class RegistroClienteRequest
    {
        [Required(ErrorMessage = "El login es obligatorio.")]
        [MaxLength(100)]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MaxLength(256)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string TipoIdentificacion { get; set; } = "CEDULA";

        [Required]
        [MaxLength(20)]
        public string NumeroIdentificacion { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }
    }
}
