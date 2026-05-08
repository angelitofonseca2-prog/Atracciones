using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Reservas
{
    public class ClienteInvitadoRequest
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
    }

    public class CrearReservaRequest
    {
        [Required(ErrorMessage = "El GUID de la atracción es obligatorio.")]
        public Guid AtGuid { get; set; }

        [Required(ErrorMessage = "El GUID del horario es obligatorio.")]
        public Guid HorGuid { get; set; }

        [Required(ErrorMessage = "Debe incluir al menos una línea de ticket.")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos una línea de ticket.")]
        public IList<ReservaDetalleRequest> Lineas { get; set; } = new List<ReservaDetalleRequest>();

        public string? OrigenCanal { get; set; }

        public ClienteInvitadoRequest? ClienteInvitado { get; set; }
    }
}
