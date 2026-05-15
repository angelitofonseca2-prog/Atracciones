using System.ComponentModel.DataAnnotations;

namespace Atracciones.MsIdentidad.Business.DTOs;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "El login es obligatorio.")]
    [MaxLength(100)]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MaxLength(256)]
    public string Password { get; set; } = string.Empty;
}
