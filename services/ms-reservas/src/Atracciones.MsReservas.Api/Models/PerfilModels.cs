using System.ComponentModel.DataAnnotations;

namespace Atracciones.MsReservas.Api.Models;

public sealed class PerfilClienteResponse
{
    public Guid CliGuid { get; set; }
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
}

public sealed class ActualizarPerfilClienteRequest
{
    [MaxLength(100)] public string? Nombres { get; set; }
    [MaxLength(100)] public string? Apellidos { get; set; }
    [MaxLength(150)][EmailAddress] public string? Correo { get; set; }
    [MaxLength(20)] public string? Telefono { get; set; }
}
