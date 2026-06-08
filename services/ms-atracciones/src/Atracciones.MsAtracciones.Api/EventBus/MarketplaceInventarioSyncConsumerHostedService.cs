using Atracciones.Contracts.Events;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.MsAtracciones.Api.EventBus;

public sealed class MarketplaceInventarioSyncConsumerHostedService : RabbitMqConsumerHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MarketplaceInventarioSyncConsumerHostedService(
        RabbitMqConnectionHolder holder,
        IOptions<EvBusOptions> evBus,
        IServiceScopeFactory scopeFactory,
        ILogger<MarketplaceInventarioSyncConsumerHostedService> logger)
        : base(holder, evBus, scopeFactory, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string QueueName => EventTypes.QueueAtraccionesMarketplaceSync;

    protected override async Task HandleMessageAsync(string body, string correlationId, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<MarketplaceInventarioSyncHandler>();
        await handler.HandleAsync(body, correlationId, ct);
    }
}
