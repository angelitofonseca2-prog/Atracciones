namespace Atracciones.MsReservas.DataManagement.Models;

public sealed class ClienteDto
{
    public Guid CliGuid { get; init; }
    public string TipoIdentificacion { get; init; } = string.Empty;
    public string NumeroIdentificacion { get; init; } = string.Empty;
    public string? Nombres { get; init; }
    public string? Apellidos { get; init; }
    public string? RazonSocial { get; init; }
    public string Correo { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
}
