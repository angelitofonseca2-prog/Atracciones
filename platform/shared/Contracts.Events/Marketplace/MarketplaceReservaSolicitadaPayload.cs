using System.Text.Json.Serialization;

namespace Atracciones.Contracts.Events.Marketplace;

public sealed class MarketplaceReservaSolicitadaPayload
{
    [JsonPropertyName("seguimiento_id")]
    public Guid SeguimientoId { get; init; }

    [JsonPropertyName("rev_guid")]
    public Guid RevGuid { get; init; }

    [JsonPropertyName("cli_guid")]
    public Guid? CliGuid { get; init; }

    [JsonPropertyName("at_guid")]
    public Guid AtGuid { get; init; }

    [JsonPropertyName("hor_guid")]
    public Guid HorGuid { get; init; }

    [JsonPropertyName("fecha_visita")]
    public string? FechaVisita { get; init; }

    [JsonPropertyName("lineas")]
    public IReadOnlyList<MarketplaceReservaLineaPayload> Lineas { get; init; } = Array.Empty<MarketplaceReservaLineaPayload>();

    [JsonPropertyName("origen_canal")]
    public string OrigenCanal { get; init; } = "MARKETPLACE";

    [JsonPropertyName("cliente_invitado")]
    public MarketplaceClienteInvitadoPayload? ClienteInvitado { get; init; }

    [JsonPropertyName("usuario_accion")]
    public string UsuarioAccion { get; init; } = "marketplace";

    [JsonPropertyName("ip_accion")]
    public string IpAccion { get; init; } = "0.0.0.0";
}
