using System.ComponentModel.DataAnnotations;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Usuarios
{
    public class CrearUsuarioRequest
    {
        [Required][MaxLength(100)] public string Login { get; set; } = string.Empty;
        [Required][MaxLength(256)] public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe asignar al menos un rol.")]
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
