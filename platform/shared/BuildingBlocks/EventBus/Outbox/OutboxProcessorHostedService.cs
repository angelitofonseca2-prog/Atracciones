using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.Platform.BuildingBlocks.EventBus.Outbox;

public sealed class OutboxProcessorHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRabbitMqPublisher _publisher;
    private readonly EvBusOptions _evBus;
    private readonly ILogger<OutboxProcessorHostedService> _logger;

    public OutboxProcessorHostedService(
        IServiceScopeFactory scopeFactory,
        IRabbitMqPublisher publisher,
        IOptions<EvBusOptions> evBus,
        ILogger<OutboxProcessorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _evBus = evBus.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_evBus.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var reader = scope.ServiceProvider.GetService<IOutboxReader>();
                if (reader is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                var pending = await reader.GetPendingAsync(20, stoppingToken);
                foreach (var msg in pending)
                {
                    try
                    {
                        _publisher.Publish(msg.RoutingKey, msg.PayloadJson, msg.CorrelationId);
                        await reader.MarkPublishedAsync(msg.ObGuid, stoppingToken);
                    }
                    catch (Exception pubEx) when (pubEx is not OperationCanceledException)
                    {
                        // Broker temporalmente no disponible: dejar el mensaje en outbox y reintentar en el siguiente ciclo.
                        _logger.LogWarning(pubEx, "No se pudo publicar mensaje {ObGuid} (routing={RoutingKey}). Se reintentará.", msg.ObGuid, msg.RoutingKey);
                        break;
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error procesando outbox");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
