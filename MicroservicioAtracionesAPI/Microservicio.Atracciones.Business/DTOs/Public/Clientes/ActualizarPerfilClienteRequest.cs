using System.ComponentModel.DataAnnotations;

namespace Microservicio.Atracciones.Business.DTOs.Public.Clientes
{
    public class ActualizarPerfilClienteRequest
    {
        [MaxLength(100)] public string? Nombres { get; set; }
        [MaxLength(100)] public string? Apellidos { get; set; }
        [MaxLength(150)][EmailAddress] public string? Correo { get; set; }
        [MaxLength(20)] public string? Telefono { get; set; }
    }
}
