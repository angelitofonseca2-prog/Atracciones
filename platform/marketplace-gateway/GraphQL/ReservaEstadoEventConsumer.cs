using System.Text.Json;
using Atracciones.Contracts.Events;
using Atracciones.MarketplaceGateway.GraphQL;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.MarketplaceGateway.GraphQL;

/// <summary>
/// Consume eventos marketplace.reserva.confirmada / marketplace.reserva.rechazada
/// desde RabbitMQ y los retransmite como subscriptions de HotChocolate.
/// Esto elimina el polling de estadoReserva en el frontend.
/// </summary>
internal sealed class ReservaEstadoEventConsumer : RabbitMqConsumerHostedService
{
    private readonly ITopicEventSender _sender;
    private readonly ILogger<ReservaEstadoEventConsumer> _log;

    public ReservaEstadoEventConsumer(
        RabbitMqConnectionHolder holder,
        IOptions<EvBusOptions> evBus,
        IServiceScopeFactory scopeFactory,
        ITopicEventSender sender,
        ILogger<ReservaEstadoEventConsumer> logger)
        : base(holder, evBus, scopeFactory, logger)
    {
        _sender = sender;
        _log = logger;
    }

    // Cola dedicada al marketplace-gateway para eventos de estado de reserva.
    protected override string QueueName => EventTypes.QueueReservasMarketplace;

    protected override async Task HandleMessageAsync(string body, string correlationId, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        if (!root.TryGetProperty("event_type", out var evTypeProp))
            return;

        var evType = evTypeProp.GetString();
        if (evType is not ("marketplace.reserva.confirmada" or "marketplace.reserva.rechazada"))
            return;

        if (!root.TryGetProperty("payload", out var payloadEl))
            return;

        if (!payloadEl.TryGetProperty("seguimiento_id", out var sidEl) ||
            !Guid.TryParse(sidEl.GetString(), out var seguimientoId))
            return;

        var estado = evType == "marketplace.reserva.confirmada" ? "CONFIRMADA" : "RECHAZADA";

        Guid? revGuid = null;
        if (payloadEl.TryGetProperty("rev_guid", out var rgEl) && rgEl.ValueKind != JsonValueKind.Null)
            Guid.TryParse(rgEl.GetString(), out var rg).Equals(revGuid = rg);

        string? revCodigo = payloadEl.TryGetProperty("rev_codigo", out var rcEl) && rcEl.ValueKind != JsonValueKind.Null
            ? rcEl.GetString()
            : null;

        string? motivoRechazo = payloadEl.TryGetProperty("motivo_rechazo", out var mrEl) && mrEl.ValueKind != JsonValueKind.Null
            ? mrEl.GetString()
            : null;

        var payload = new EstadoReservaPayload
        {
            SeguimientoId = seguimientoId,
            RevGuid = revGuid,
            RevCodigo = revCodigo,
            Estado = estado,
            MotivoRechazo = motivoRechazo,
            CorrelationId = correlationId,
        };

        var topic = $"seguimiento:{seguimientoId:D}";
        await _sender.SendAsync(topic, payload, ct);
        _log.LogInformation("Subscription enviada: {Topic} Estado={Estado}", topic, estado);
    }
}
