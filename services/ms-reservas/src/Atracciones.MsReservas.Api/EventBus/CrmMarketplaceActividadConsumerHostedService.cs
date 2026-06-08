using Atracciones.Contracts.Events;
using Atracciones.Contracts.Events.Marketplace;
using Atracciones.MsReservas.DataAccess.Repositories;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.MsReservas.Api.EventBus;

public sealed class CrmMarketplaceActividadConsumerHostedService : RabbitMqConsumerHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CrmMarketplaceActividadConsumerHostedService> _actividadLogger;

    public CrmMarketplaceActividadConsumerHostedService(
        RabbitMqConnectionHolder holder,
        IOptions<EvBusOptions> evBus,
        IServiceScopeFactory scopeFactory,
        ILogger<CrmMarketplaceActividadConsumerHostedService> logger)
        : base(holder, evBus, scopeFactory, logger)
    {
        _scopeFactory = scopeFactory;
        _actividadLogger = logger;
    }

    protected override string QueueName => EventTypes.QueueCrmMarketplaceActividad;

    protected override async Task HandleMessageAsync(string body, string correlationId, CancellationToken ct)
    {
        var envelope = EventEnvelope<MarketplaceReservaConfirmadaPayload>.FromJson(body);
        if (envelope is null)
            return;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var clientes = scope.ServiceProvider.GetRequiredService<DataManagement.Interfaces.IClienteRepository>();
        var cliente = await clientes.ObtenerActivoPorGuidAsync(envelope.Payload.CliGuid, ct);
        _actividadLogger.LogInformation(
            "Actividad marketplace: reserva confirmada {RevCodigo} cliente {Correo} correlation={CorrelationId}",
            envelope.Payload.RevCodigo,
            cliente?.Correo ?? envelope.Payload.CliGuid.ToString("D"),
            correlationId);
        await Task.CompletedTask;
    }
}
