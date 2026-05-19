using System.ComponentModel.DataAnnotations;

namespace Atracciones.MsReservas.Api.Models.Admin;

public sealed class ActualizarEstadoReservaRequest
{
    [Required]
    [RegularExpression("^[AIC]$", ErrorMessage = "Estado inválido. Valores: A, I, C.")]
    public char NuevoEstado { get; set; }

    public string Motivo { get; set; } = string.Empty;
}
