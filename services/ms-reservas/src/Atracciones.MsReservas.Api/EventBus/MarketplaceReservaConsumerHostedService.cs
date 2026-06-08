using Atracciones.Contracts.Events;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.MsReservas.Api.EventBus;

public sealed class MarketplaceReservaConsumerHostedService : RabbitMqConsumerHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MarketplaceReservaConsumerHostedService(
        RabbitMqConnectionHolder holder,
        IOptions<EvBusOptions> evBus,
        IServiceScopeFactory scopeFactory,
        ILogger<MarketplaceReservaConsumerHostedService> logger)
        : base(holder, evBus, scopeFactory, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string QueueName => EventTypes.QueueReservasMarketplace;

    protected override async Task HandleMessageAsync(string body, string correlationId, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<MarketplaceReservaEventHandler>();
        await handler.HandleSolicitadaAsync(body, correlationId, ct);
    }
}
