using Atracciones.MsClientes.DataManagement.Models;

namespace Atracciones.MsClientes.DataManagement.Interfaces;

public interface IClienteRepository
{
    Task<ClienteDto?> ObtenerActivoPorGuidAsync(Guid cliGuid, CancellationToken ct = default);
    Task<ClienteDto?> ObtenerActivoPorNumeroIdentificacionAsync(string numeroIdentificacion, CancellationToken ct = default);
    Task UpsertMirrorAsync(ClienteMirrorDto dto, CancellationToken ct = default);
    Task<ClienteDto> CrearAsync(CrearClienteInternoDto dto, CancellationToken ct = default);
    Task<ClienteDto?> ActualizarCamposAsync(Guid cliGuid, ActualizarClienteInternoDto dto, CancellationToken ct = default);
    Task<bool> MarcarInactivoAsync(Guid cliGuid, CancellationToken ct = default);
}

public sealed class CrearClienteInternoDto
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
    public string CreadoPor { get; init; } = string.Empty;
    public string IpCreador { get; init; } = string.Empty;
}

public sealed class ActualizarClienteInternoDto
{
    public string? TipoIdentificacion { get; init; }
    public string? NumeroIdentificacion { get; init; }
    public string? Nombres { get; init; }
    public string? Apellidos { get; init; }
    public string? RazonSocial { get; init; }
    public string? Correo { get; init; }
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
}
