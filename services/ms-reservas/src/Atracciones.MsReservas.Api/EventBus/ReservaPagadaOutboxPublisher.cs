using Atracciones.Contracts.Events;
using Atracciones.Contracts.Events.Marketplace;
using Atracciones.MsReservas.DataAccess.Context;
using Atracciones.MsReservas.DataAccess.Entities;
using Atracciones.MsReservas.DataManagement.Models;
using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Microsoft.Extensions.Options;

namespace Atracciones.MsReservas.Api.EventBus;

public sealed class ReservaPagadaOutboxPublisher
{
    private readonly VentasDbContext _db;
    private readonly EvBusOptions _evBus;

    public ReservaPagadaOutboxPublisher(VentasDbContext db, IOptions<EvBusOptions> evBus)
    {
        _db = db;
        _evBus = evBus.Value;
    }

    public async Task TryEnqueueAsync(
        ReservaDetalladaDto reserva,
        string nombreReceptor,
        string correoReceptor,
        string? telefonoReceptor,
        string correlationId,
        CancellationToken ct = default)
    {
        if (!_evBus.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(nombreReceptor) || string.IsNullOrWhiteSpace(correoReceptor))
            return;

        var corr = string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("D") : correlationId;
        var payload = EventEnvelope<ReservasReservaPagadaPayload>.Create(
            EventTypes.ReservasReservaPagada,
            new ReservasReservaPagadaPayload
            {
                RevGuid = reserva.RevGuid,
                CliGuid = reserva.CliGuid,
                RevCodigo = reserva.RevCodigo,
                Total = reserva.Total,
                NombreReceptor = nombreReceptor.Trim(),
                CorreoReceptor = correoReceptor.Trim(),
                TelefonoReceptor = string.IsNullOrWhiteSpace(telefonoReceptor) ? null : telefonoReceptor.Trim(),
            },
            corr);

        _db.OutboxEvents.Add(new OutboxEventEntity
        {
            ObGuid = Guid.NewGuid(),
            RoutingKey = EventTypes.ReservasReservaPagada,
            PayloadJson = payload.ToJson(),
            CorrelationId = corr,
            CreatedUtc = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
    }
}
