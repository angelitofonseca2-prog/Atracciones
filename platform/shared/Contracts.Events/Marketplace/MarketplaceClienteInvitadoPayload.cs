using System.Text.Json.Serialization;

namespace Atracciones.Contracts.Events.Marketplace;

public sealed class MarketplaceClienteInvitadoPayload
{
    [JsonPropertyName("tipo_identificacion")]
    public string TipoIdentificacion { get; init; } = string.Empty;

    [JsonPropertyName("numero_identificacion")]
    public string NumeroIdentificacion { get; init; } = string.Empty;

    [JsonPropertyName("nombres")]
    public string? Nombres { get; init; }

    [JsonPropertyName("apellidos")]
    public string? Apellidos { get; init; }

    [JsonPropertyName("correo")]
    public string Correo { get; init; } = string.Empty;

    [JsonPropertyName("telefono")]
    public string? Telefono { get; init; }

    [JsonPropertyName("direccion")]
    public string? Direccion { get; init; }
}
