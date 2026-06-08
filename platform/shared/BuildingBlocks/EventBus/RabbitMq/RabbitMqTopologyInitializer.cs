using Atracciones.Contracts.Events;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;

public sealed class RabbitMqTopologyInitializer : IHostedService
{
    private readonly RabbitMqConnectionHolder _holder;
    private readonly EvBusOptions _evBus;
    private readonly ILogger<RabbitMqTopologyInitializer> _logger;

    public RabbitMqTopologyInitializer(
        RabbitMqConnectionHolder holder,
        IOptions<EvBusOptions> evBus,
        ILogger<RabbitMqTopologyInitializer> logger)
    {
        _holder = holder;
        _evBus = evBus.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_evBus.Enabled)
            return Task.CompletedTask;

        try
        {
            if (!_holder.TryGetConnection(out var conn) || conn is null)
            {
                _logger.LogWarning(
                    "RabbitMQ no disponible al iniciar — topología no declarada. " +
                    "El EvBus continuará desactivado hasta reconexión.");
                return Task.CompletedTask;
            }

            using var channel = conn.CreateChannelAsync(cancellationToken: cancellationToken)
                .GetAwaiter().GetResult();

            channel.ExchangeDeclareAsync(EventTypes.DeadLetterExchange, ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken)
                .GetAwaiter().GetResult();
            channel.QueueDeclareAsync("atracciones.dlq", durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken)
                .GetAwaiter().GetResult();
            channel.QueueBindAsync("atracciones.dlq", EventTypes.DeadLetterExchange, routingKey: string.Empty, cancellationToken: cancellationToken)
                .GetAwaiter().GetResult();

            var queueArgs = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = EventTypes.DeadLetterExchange,
            };

            channel.ExchangeDeclareAsync(EventTypes.ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken)
                .GetAwaiter().GetResult();

            DeclareQueue(channel, EventTypes.QueueReservasMarketplace, EventTypes.MarketplaceReservaSolicitada, queueArgs, cancellationToken);
            DeclareQueue(channel, EventTypes.QueueAtraccionesMarketplaceSync, "marketplace.reserva.*", queueArgs, cancellationToken);
            DeclareQueue(channel, EventTypes.QueueCrmMarketplaceActividad, EventTypes.MarketplaceReservaConfirmada, queueArgs, cancellationToken);
            DeclareQueue(channel, EventTypes.QueueAuditMarketplace, "marketplace.#", queueArgs, cancellationToken);
            DeclareQueue(channel, EventTypes.QueueFacturacionReservasPagadas, EventTypes.ReservasReservaPagada, queueArgs, cancellationToken);

            _logger.LogInformation("Topología RabbitMQ declarada en exchange {Exchange}", EventTypes.ExchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Error declarando topología RabbitMQ — el servicio arrancará sin EvBus. " +
                "Verificar conexión al broker y reiniciar cuando esté disponible.");
        }

        return Task.CompletedTask;
    }

    private static void DeclareQueue(IChannel channel, string queueName, string bindingKey, IDictionary<string, object?> args, CancellationToken ct)
    {
        channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: args, cancellationToken: ct)
            .GetAwaiter().GetResult();
        channel.QueueBindAsync(queueName, EventTypes.ExchangeName, bindingKey, cancellationToken: ct)
            .GetAwaiter().GetResult();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
