using System.Text.Json;
using Atracciones.Contracts.Events.Marketplace;
using Atracciones.MarketplaceGateway.Services;
using HotChocolate;
using HotChocolate.Subscriptions;

namespace Atracciones.MarketplaceGateway.GraphQL;

public sealed class Query
{
    public async Task<string> Atracciones(
        [Service] AtraccionesProxyService proxy,
        string? ciudad,
        string? tipo,
        string? subtipo,
        string? idioma,
        double? calificacionMin,
        bool? disponible,
        string? ordenarPor,
        int page = 1,
        int limit = 10,
        CancellationToken ct = default)
    {
        var doc = await proxy.GetAtraccionesAsync(ciudad, tipo, subtipo, idioma, calificacionMin, disponible, ordenarPor, page, limit, ct);
        return doc.GetRawText();
    }

    public async Task<string> Filtros(
        [Service] AtraccionesProxyService proxy,
        string? ciudad,
        CancellationToken ct = default)
    {
        var doc = await proxy.GetFiltrosAsync(ciudad, ct);
        return doc.GetRawText();
    }

    public async Task<string> Atraccion(
        [Service] AtraccionesProxyService proxy,
        Guid guid,
        CancellationToken ct = default)
    {
        var doc = await proxy.GetAtraccionAsync(guid, ct);
        return doc.GetRawText();
    }

    public async Task<string> Horarios(
        [Service] AtraccionesProxyService proxy,
        Guid atGuid,
        bool disponibles = true,
        CancellationToken ct = default)
    {
        var doc = await proxy.GetHorariosAsync(atGuid, disponibles, ct);
        return doc.GetRawText();
    }

    public async Task<string> Tickets(
        [Service] AtraccionesProxyService proxy,
        Guid atGuid,
        CancellationToken ct = default)
    {
        var doc = await proxy.GetTicketsAsync(atGuid, ct);
        return doc.GetRawText();
    }

    public async Task<EstadoReservaPayload?> EstadoReserva(
        [Service] ReservasProxyService proxy,
        Guid seguimientoId,
        CancellationToken ct = default)
    {
        var json = await proxy.GetEstadoReservaAsync(seguimientoId, ct);
        if (json is null)
            return null;

        var data = json.Value.GetProperty("data");
        return new EstadoReservaPayload
        {
            SeguimientoId = Guid.Parse(data.GetProperty("seguimiento_id").GetString()!),
            RevGuid = data.TryGetProperty("rev_guid", out var rg) && rg.ValueKind != JsonValueKind.Null
                ? Guid.Parse(rg.GetString()!)
                : null,
            RevCodigo = data.TryGetProperty("rev_codigo", out var rc) && rc.ValueKind != JsonValueKind.Null
                ? rc.GetString()
                : null,
            Estado = data.GetProperty("estado").GetString() ?? "EN_PROCESO",
            MotivoRechazo = data.TryGetProperty("motivo_rechazo", out var mr) && mr.ValueKind != JsonValueKind.Null
                ? mr.GetString()
                : null,
            CorrelationId = data.GetProperty("correlation_id").GetString() ?? string.Empty,
        };
    }
}

public sealed class EstadoReservaPayload
{
    public Guid SeguimientoId { get; init; }
    public Guid? RevGuid { get; init; }
    public string? RevCodigo { get; init; }
    public string Estado { get; init; } = "EN_PROCESO";
    public string? MotivoRechazo { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
}

public sealed class SolicitarReservaLineaInput
{
    public Guid TckGuid { get; init; }
    public int Cantidad { get; init; }
}

public sealed class ClienteInvitadoInput
{
    public string TipoIdentificacion { get; init; } = string.Empty;
    public string NumeroIdentificacion { get; init; } = string.Empty;
    public string? Nombres { get; init; }
    public string? Apellidos { get; init; }
    public string Correo { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string? Direccion { get; init; }
}

public sealed class SolicitarReservaInput
{
    public Guid? CliGuid { get; init; }
    public Guid AtGuid { get; init; }
    public Guid HorGuid { get; init; }
    public string? FechaVisita { get; init; }
    public IReadOnlyList<SolicitarReservaLineaInput> Lineas { get; init; } = Array.Empty<SolicitarReservaLineaInput>();
    public ClienteInvitadoInput? ClienteInvitado { get; init; }
    public string OrigenCanal { get; init; } = "MARKETPLACE";
}

public sealed class SolicitudReservaPayload
{
    public Guid SeguimientoId { get; init; }
    public Guid RevGuid { get; init; }
    public string Estado { get; init; } = "EN_PROCESO";
    public string CorrelationId { get; init; } = string.Empty;
}

/// <summary>
/// Subscription para recibir actualizaciones de estado en tiempo real.
/// Reemplaza el polling de estadoReserva cuando el cliente usa WebSocket.
/// </summary>
public sealed class Subscription
{
    /// <summary>
    /// Se activa cuando ms-reservas procesa la reserva (CONFIRMADA o RECHAZADA).
    /// Tópico: "seguimiento:{seguimientoId}" — emitido por <see cref="ReservaEstadoEventConsumer"/>.
    /// </summary>
    [Subscribe]
    [Topic("seguimiento:{seguimientoId}")]
    public EstadoReservaPayload OnEstadoReservaActualizado(
        Guid seguimientoId,
        [EventMessage] EstadoReservaPayload payload)
        => payload;
}

public sealed class Mutation
{
    public SolicitudReservaPayload SolicitarReserva(
        [Service] MarketplaceReservaPublisher publisher,
        [Service] IHttpContextAccessor httpContextAccessor,
        SolicitarReservaInput input)
    {
        var corr = httpContextAccessor.HttpContext?.Items["correlationId"]?.ToString()
            ?? Guid.NewGuid().ToString("D");

        var seguimientoId = Guid.NewGuid();
        var revGuid = Guid.NewGuid();

        var payload = new MarketplaceReservaSolicitadaPayload
        {
            SeguimientoId = seguimientoId,
            RevGuid = revGuid,
            CliGuid = input.CliGuid,
            AtGuid = input.AtGuid,
            HorGuid = input.HorGuid,
            FechaVisita = input.FechaVisita,
            OrigenCanal = input.OrigenCanal,
            Lineas = input.Lineas.Select(l => new MarketplaceReservaLineaPayload
            {
                TckGuid = l.TckGuid,
                Cantidad = l.Cantidad,
            }).ToList(),
            ClienteInvitado = input.ClienteInvitado is null
                ? null
                : new MarketplaceClienteInvitadoPayload
                {
                    TipoIdentificacion = input.ClienteInvitado.TipoIdentificacion,
                    NumeroIdentificacion = input.ClienteInvitado.NumeroIdentificacion,
                    Nombres = input.ClienteInvitado.Nombres,
                    Apellidos = input.ClienteInvitado.Apellidos,
                    Correo = input.ClienteInvitado.Correo,
                    Telefono = input.ClienteInvitado.Telefono,
                    Direccion = input.ClienteInvitado.Direccion,
                },
        };

        publisher.PublishSolicitud(payload, corr);

        return new SolicitudReservaPayload
        {
            SeguimientoId = seguimientoId,
            RevGuid = revGuid,
            Estado = "EN_PROCESO",
            CorrelationId = corr,
        };
    }
}
