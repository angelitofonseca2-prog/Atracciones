namespace Atracciones.MsClientes.DataManagement.Models;

/// <summary>cli_guid en CRM = usu_guid del monolito (Fase 2).</summary>
public sealed class ClienteMirrorDto
{
    public Guid UsuGuid { get; init; }
    public string TipoIdentificacion { get; init; } = string.Empty;
    public string NumeroIdentificacion { get; init; } = string.Empty;
    public string? Nombres { get; init; }
    public string? Apellidos { get; init; }
    public string? RazonSocial { get; init; }
    public string Correo { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
    public string CreadoPor { get; init; } = string.Empty;
    public string IpCreador { get; init; } = string.Empty;
}
