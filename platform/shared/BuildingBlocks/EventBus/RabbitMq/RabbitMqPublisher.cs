using Atracciones.Contracts.Events;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;

public interface IRabbitMqPublisher
{
    void Publish(string routingKey, string bodyJson, string correlationId);
}

public sealed class RabbitMqConnectionHolder : IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly object _lock = new();
    private IConnection? _connection;

    public RabbitMqConnectionHolder(IOptions<RabbitMqOptions> options) =>
        _options = options.Value;

    /// <summary>
    /// Devuelve la conexión activa. Si el broker no está disponible lanza excepción;
    /// el llamador decide si es fatal o tolerable (startup vs runtime).
    /// </summary>
    public IConnection GetConnection()
    {
        lock (_lock)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.Username,
                Password = _options.Password,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(8),
                RequestedHeartbeat = TimeSpan.FromSeconds(30),
            };
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            return _connection;
        }
    }

    /// <summary>
    /// Intenta obtener la conexión sin lanzar excepción — para arranque tolerante.
    /// Devuelve false si el broker no está disponible.
    /// </summary>
    public bool TryGetConnection(out IConnection? connection)
    {
        try
        {
            connection = GetConnection();
            return true;
        }
        catch
        {
            connection = null;
            return false;
        }
    }

    public void Dispose()
    {
        try { _connection?.Dispose(); } catch { /* ignore */ }
    }
}

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly RabbitMqConnectionHolder _holder;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly object _channelLock = new();
    private IChannel? _channel;

    public RabbitMqPublisher(RabbitMqConnectionHolder holder, ILogger<RabbitMqPublisher> logger)
    {
        _holder = holder;
        _logger = logger;
    }

    public void Publish(string routingKey, string bodyJson, string correlationId)
    {
        var channel = GetOrRecreateChannel();
        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            CorrelationId = correlationId,
        };
        var body = System.Text.Encoding.UTF8.GetBytes(bodyJson);
        try
        {
            channel.BasicPublishAsync(
                exchange: EventTypes.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body).GetAwaiter().GetResult();
            _logger.LogDebug("Publicado {RoutingKey} correlation={CorrelationId}", routingKey, correlationId);
        }
        catch
        {
            // Invalidar canal para que se recree en el próximo intento.
            lock (_channelLock) { _channel = null; }
            throw;
        }
    }

    private IChannel GetOrRecreateChannel()
    {
        lock (_channelLock)
        {
            if (_channel is { IsOpen: true })
                return _channel;

            try { _channel?.Dispose(); } catch { /* ignore */ }
            _channel = _holder.GetConnection().CreateChannelAsync().GetAwaiter().GetResult();
            return _channel;
        }
    }

    public void Dispose()
    {
        lock (_channelLock)
        {
            try { _channel?.Dispose(); } catch { /* ignore */ }
        }
    }
}
