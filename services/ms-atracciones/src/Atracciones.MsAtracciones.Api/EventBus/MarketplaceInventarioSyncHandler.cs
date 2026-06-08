using Atracciones.Contracts.Events;
using Atracciones.Contracts.Events.Marketplace;
using Atracciones.MsAtracciones.DataAccess.Context;
using Atracciones.MsAtracciones.DataAccess.Entities;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Atracciones.MsAtracciones.Api.EventBus;

public sealed class MarketplaceInventarioSyncHandler
{
    private readonly InventarioDbContext _db;
    private readonly IOutboxWriter _outbox;
    private readonly ILogger<MarketplaceInventarioSyncHandler> _logger;

    public MarketplaceInventarioSyncHandler(
        InventarioDbContext db,
        IOutboxWriter outbox,
        ILogger<MarketplaceInventarioSyncHandler> logger)
    {
        _db = db;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task HandleAsync(string body, string correlationId, CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(body);
        var eventType = doc.RootElement.GetProperty("event_type").GetString() ?? string.Empty;

        if (eventType == EventTypes.MarketplaceReservaConfirmada)
        {
            var envelope = EventEnvelope<MarketplaceReservaConfirmadaPayload>.FromJson(body);
            if (envelope is null) return;
            await VerificarCupoAgotadoAsync(envelope.Payload.HorGuid, envelope.CorrelationId, ct);
            return;
        }

        if (eventType == EventTypes.MarketplaceReservaRechazada)
        {
            _logger.LogInformation("Reserva marketplace rechazada recibida en inventario sync.");
        }
    }

    private async Task VerificarCupoAgotadoAsync(Guid horGuid, string correlationId, CancellationToken ct)
    {
        var hor = await _db.Horarios.AsNoTracking()
            .Include(h => h.Ticket)
            .FirstOrDefaultAsync(h => h.HorGuid == horGuid, ct);
        if (hor is null || hor.HorCuposDisponibles > 0)
            return;

        var payload = EventEnvelope<object>.Create(
            EventTypes.AtraccionesHorarioCupoAgotado,
            new { hor_guid = horGuid, at_guid = hor.Ticket.AtGuid },
            correlationId);

        await _outbox.EnqueueAsync(EventTypes.AtraccionesHorarioCupoAgotado, payload.ToJson(), correlationId, ct);
        _logger.LogInformation("Cupo agotado publicado para horario {HorGuid}", horGuid);
    }
}
