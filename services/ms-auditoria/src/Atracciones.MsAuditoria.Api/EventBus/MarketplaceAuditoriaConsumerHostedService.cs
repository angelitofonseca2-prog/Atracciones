using Atracciones.Contracts.Events;
using Atracciones.MsAuditoria.DataManagement.Interfaces;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.MsAuditoria.Api.EventBus;

public sealed class MarketplaceAuditoriaConsumerHostedService : RabbitMqConsumerHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MarketplaceAuditoriaConsumerHostedService(
        RabbitMqConnectionHolder holder,
        IOptions<EvBusOptions> evBus,
        IServiceScopeFactory scopeFactory,
        ILogger<MarketplaceAuditoriaConsumerHostedService> logger)
        : base(holder, evBus, scopeFactory, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string QueueName => EventTypes.QueueAuditMarketplace;

    protected override async Task HandleMessageAsync(string body, string correlationId, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAuditoriaRepository>();
        var eventType = ExtractEventType(body);
        await repo.RegistrarEventoAsync(
            string.IsNullOrWhiteSpace(eventType) ? "marketplace.evento" : eventType,
            correlationId,
            body,
            ct);
    }

    private static string ExtractEventType(string body)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("event_type", out var t))
                return t.GetString() ?? string.Empty;
        }
        catch { /* ignore */ }
        return string.Empty;
    }
}
