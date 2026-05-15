namespace Atracciones.MsOrquestador.Business.Services;

public sealed record RegistroOrquestadorDto
{
    public string Login { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string TipoIdentificacion { get; init; } = string.Empty;
    public string NumeroIdentificacion { get; init; } = string.Empty;
    public string Nombres { get; init; } = string.Empty;
    public string Apellidos { get; init; } = string.Empty;
    public string Correo { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string IpCreador { get; init; } = "0.0.0.0";
}

public sealed record RegistroOrquestadorResultDto
{
    public string Token { get; init; } = string.Empty;
    public string Login { get; init; } = string.Empty;
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}

public interface IRegistroOrquestacionService
{
    Task<RegistroOrquestadorResultDto> RegistrarAsync(
        RegistroOrquestadorDto dto,
        string correlationId,
        CancellationToken ct);
}
