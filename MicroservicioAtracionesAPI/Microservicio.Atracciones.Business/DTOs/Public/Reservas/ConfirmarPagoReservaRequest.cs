using System.ComponentModel.DataAnnotations;

namespace Microservicio.Atracciones.Business.DTOs.Public.Reservas
{
    public class ConfirmarPagoReservaRequest
    {
        [Required]
        [MaxLength(100)]
        public string NombreReceptor { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ApellidoReceptor { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string CorreoReceptor { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? TelefonoReceptor { get; set; }

        [MaxLength(500)]
        public string? Observacion { get; set; }
    }
}
