using Atracciones.Contracts.Events;
using Atracciones.Contracts.Events.Marketplace;
using Atracciones.MsFacturacion.DataManagement.Interfaces;
using Atracciones.MsFacturacion.DataManagement.Models;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atracciones.MsFacturacion.Api.EventBus;

public sealed class ReservaPagadaConsumerHostedService : RabbitMqConsumerHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReservaPagadaConsumerHostedService(
        RabbitMqConnectionHolder holder,
        IOptions<EvBusOptions> evBus,
        IServiceScopeFactory scopeFactory,
        ILogger<ReservaPagadaConsumerHostedService> logger)
        : base(holder, evBus, scopeFactory, logger)
    {
        _scopeFactory = scopeFactory;
    }

    protected override string QueueName => EventTypes.QueueFacturacionReservasPagadas;

    protected override async Task HandleMessageAsync(string body, string correlationId, CancellationToken ct)
    {
        var envelope = EventEnvelope<ReservasReservaPagadaPayload>.FromJson(body);
        if (envelope is null)
            return;

        var p = envelope.Payload;
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IFacturaRepository>();

        await repo.EmitirAsync(new EmitirFacturaInternaDto
        {
            RevGuid = p.RevGuid,
            CliGuid = p.CliGuid,
            NombreReceptor = p.NombreReceptor,
            CorreoReceptor = p.CorreoReceptor,
            TelefonoReceptor = p.TelefonoReceptor ?? string.Empty,
            Total = p.Total,
            Moneda = "USD",
            RevCodigoSnap = p.RevCodigo,
            UsuarioEmision = "evbus",
            IpEmision = "0.0.0.0",
        }, ct);
    }
}
