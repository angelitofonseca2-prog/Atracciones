using System.Text.Json;
using Atracciones.Contracts.Events;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;

public abstract class RabbitMqConsumerHostedService : BackgroundService
{
    private readonly RabbitMqConnectionHolder _holder;
    private readonly EvBusOptions _evBus;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    protected RabbitMqConsumerHostedService(
        RabbitMqConnectionHolder holder,
        IOptions<EvBusOptions> evBus,
        IServiceScopeFactory scopeFactory,
        ILogger logger)
    {
        _holder = holder;
        _evBus = evBus.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected abstract string QueueName { get; }

    protected abstract Task HandleMessageAsync(string body, string correlationId, CancellationToken ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_evBus.Enabled)
            return;

        // Espera inicial para que el TopologyInitializer declare las colas primero.
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        var backoffSeconds = new[] { 5, 10, 20, 30, 60 };
        var attempt = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            IChannel? channel = null;
            try
            {
                if (!_holder.TryGetConnection(out var conn) || conn is null)
                {
                    var delay = backoffSeconds[Math.Min(attempt++, backoffSeconds.Length - 1)];
                    _logger.LogWarning("Consumidor {Queue}: broker no disponible, reintentando en {Delay}s (intento {Attempt})", QueueName, delay, attempt);
                    await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
                    continue;
                }

                attempt = 0;
                channel = await conn.CreateChannelAsync(cancellationToken: stoppingToken);
                await channel.BasicQosAsync(0, 1, false, stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    var body = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                    var correlationId = ea.BasicProperties?.CorrelationId ?? string.Empty;

                    try
                    {
                        if (TryGetEventId(body, out var eventId, out var eventType))
                        {
                            await using var scope = _scopeFactory.CreateAsyncScope();
                            var store = scope.ServiceProvider.GetRequiredService<IProcessedEventStore>();
                            if (!await store.TryMarkProcessedAsync(eventId, eventType, stoppingToken))
                            {
                                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                                return;
                            }
                        }

                        await HandleMessageAsync(body, correlationId, stoppingToken);
                        await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error consumiendo mensaje de cola {Queue}", QueueName);
                        try { await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false, stoppingToken); } catch { /* ignore */ }
                    }
                };

                await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
                _logger.LogInformation("Consumidor activo en cola {Queue}", QueueName);

                // Mantener activo hasta cancelación o pérdida de canal.
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                var delay = backoffSeconds[Math.Min(attempt++, backoffSeconds.Length - 1)];
                _logger.LogWarning(ex, "Consumidor {Queue} desconectado, reintentando en {Delay}s", QueueName, delay);
                try { channel?.Dispose(); } catch { /* ignore */ }
                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
        }
    }

    private static bool TryGetEventId(string body, out Guid eventId, out string eventType)
    {
        eventId = Guid.Empty;
        eventType = string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("event_id", out var idEl) &&
                Guid.TryParse(idEl.GetString(), out eventId))
            {
                if (doc.RootElement.TryGetProperty("event_type", out var typeEl))
                    eventType = typeEl.GetString() ?? string.Empty;
                return true;
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }
}
