using System.Text.Json.Serialization;

namespace Atracciones.Contracts.Events.Marketplace;

public sealed class ReservasReservaPagadaPayload
{
    [JsonPropertyName("rev_guid")]
    public Guid RevGuid { get; init; }

    [JsonPropertyName("cli_guid")]
    public Guid CliGuid { get; init; }

    [JsonPropertyName("rev_codigo")]
    public string RevCodigo { get; init; } = string.Empty;

    [JsonPropertyName("total")]
    public decimal Total { get; init; }

    [JsonPropertyName("nombre_receptor")]
    public string NombreReceptor { get; init; } = string.Empty;

    [JsonPropertyName("correo_receptor")]
    public string CorreoReceptor { get; init; } = string.Empty;

    [JsonPropertyName("telefono_receptor")]
    public string? TelefonoReceptor { get; init; }
}
