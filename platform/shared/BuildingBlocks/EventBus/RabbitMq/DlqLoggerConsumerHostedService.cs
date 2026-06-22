using Atracciones.Contracts.Events;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;

/// <summary>
/// Consume la Dead-Letter Queue y registra cada mensaje fallido en los logs.
/// Registrar como HostedService en los microservicios que usen EvBus para tener
/// visibilidad inmediata de mensajes que no pudieron procesarse.
/// </summary>
public sealed class DlqLoggerConsumerHostedService : RabbitMqConsumerHostedService
{
    private readonly ILogger<DlqLoggerConsumerHostedService> _log;

    public DlqLoggerConsumerHostedService(
        RabbitMqConnectionHolder holder,
        IOptions<EvBusOptions> evBus,
        IServiceScopeFactory scopeFactory,
        ILogger<DlqLoggerConsumerHostedService> logger)
        : base(holder, evBus, scopeFactory, logger)
    {
        _log = logger;
    }

    protected override string QueueName => EventTypes.DlqQueueName;

    protected override Task HandleMessageAsync(string body, string correlationId, CancellationToken ct)
    {
        // Extrae event_type si existe para facilitar el diagnóstico.
        string eventType = "unknown";
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("event_type", out var et))
                eventType = et.GetString() ?? eventType;
        }
        catch { /* ignore */ }

        _log.LogError(
            "[DLQ] Mensaje en dead-letter queue. EventType={EventType} Correlation={CorrelationId} Body={Body}",
            eventType, correlationId, body.Length > 500 ? body[..500] + "…" : body);

        // Hacer ACK: el mensaje ya está en DLQ, loguear es suficiente.
        // Si necesitas requeue manual, hazlo desde la consola de RabbitMQ.
        return Task.CompletedTask;
    }
}
