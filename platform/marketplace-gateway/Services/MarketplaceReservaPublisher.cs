using Atracciones.Contracts.Events;
using Atracciones.Contracts.Events.Marketplace;
using Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;

namespace Atracciones.MarketplaceGateway.Services;

public sealed class MarketplaceReservaPublisher
{
    private readonly IRabbitMqPublisher _publisher;

    public MarketplaceReservaPublisher(IRabbitMqPublisher publisher) => _publisher = publisher;

    public Guid PublishSolicitud(MarketplaceReservaSolicitadaPayload payload, string correlationId)
    {
        var eventId = Guid.NewGuid();
        var envelope = EventEnvelope<MarketplaceReservaSolicitadaPayload>.Create(
            EventTypes.MarketplaceReservaSolicitada,
            payload,
            correlationId,
            eventId);
        _publisher.Publish(EventTypes.MarketplaceReservaSolicitada, envelope.ToJson(), correlationId);
        return eventId;
    }
}
