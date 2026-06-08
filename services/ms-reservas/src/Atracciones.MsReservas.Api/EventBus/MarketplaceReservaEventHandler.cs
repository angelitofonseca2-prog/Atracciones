using Atracciones.Contracts.Events;
using Atracciones.Contracts.Events.Marketplace;
using Atracciones.Contracts.Inventario.V1;
using Atracciones.MsReservas.DataAccess.Context;
using Atracciones.MsReservas.DataAccess.Entities;
using Atracciones.MsReservas.DataAccess.Repositories;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Atracciones.MsReservas.DataManagement.Models;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Atracciones.MsReservas.Api.EventBus;

public sealed class MarketplaceReservaEventHandler
{
    private readonly AtraccionInventarioService.AtraccionInventarioServiceClient _inv;
    private readonly IReservaRepository _reservas;
    private readonly IClienteRepository _clientes;
    private readonly IMarketplaceSeguimientoRepository _seguimiento;
    private readonly VentasDbContext _db;
    private readonly ILogger<MarketplaceReservaEventHandler> _logger;

    public MarketplaceReservaEventHandler(
        AtraccionInventarioService.AtraccionInventarioServiceClient inv,
        IReservaRepository reservas,
        IClienteRepository clientes,
        IMarketplaceSeguimientoRepository seguimiento,
        VentasDbContext db,
        ILogger<MarketplaceReservaEventHandler> logger)
    {
        _inv = inv;
        _reservas = reservas;
        _clientes = clientes;
        _seguimiento = seguimiento;
        _db = db;
        _logger = logger;
    }

    public async Task HandleSolicitadaAsync(string body, string correlationId, CancellationToken ct)
    {
        var envelope = EventEnvelope<MarketplaceReservaSolicitadaPayload>.FromJson(body)
            ?? throw new InvalidOperationException("Evento inválido.");

        var p = envelope.Payload;
        var corr = string.IsNullOrWhiteSpace(correlationId) ? envelope.CorrelationId : correlationId;

        try
        {
            await _seguimiento.CrearEnProcesoAsync(p.SeguimientoId, corr, ct);
        }
        catch (DbUpdateException)
        {
            _logger.LogInformation("Seguimiento {SeguimientoId} ya existe; idempotente.", p.SeguimientoId);
            return;
        }

        var cliGuid = await ResolverCliGuidAsync(p, ct);
        var prep = await PrepararLineasAsync(p, ct);
        var totalPersonas = p.Lineas.Sum(l => l.Cantidad);

        var cupo = await _inv.ValidarYReservarCupoAsync(new ValidarYReservarCupoRequest
        {
            HorGuid = p.HorGuid.ToString("D"),
            CantidadPersonas = totalPersonas,
            ReservaGuid = p.RevGuid.ToString("D"),
        }, cancellationToken: ct);

        if (!cupo.Ok)
        {
            await RechazarAsync(p, string.IsNullOrWhiteSpace(cupo.Mensaje) ? "Sin cupos suficientes." : cupo.Mensaje, corr, ct);
            return;
        }

        try
        {
            var creada = await _reservas.CrearPendienteAsync(new CrearReservaInternaDto
            {
                RevGuidPreasignado = p.RevGuid,
                CliGuid = cliGuid,
                AtGuid = p.AtGuid,
                HorGuid = p.HorGuid,
                Subtotal = prep.Subtotal,
                ValorIva = prep.Iva,
                Total = prep.Total,
                OrigenCanal = p.OrigenCanal,
                UsuarioIngreso = p.UsuarioAccion,
                IpIngreso = p.IpAccion,
                AtraccionNombreSnap = prep.AtraccionNombre,
                HorFechaSnap = prep.HorFecha,
                HorHoraInicioSnap = prep.HorHoraInicio,
                HorHoraFinSnap = prep.HorHoraFin,
                Lineas = prep.Lineas,
            }, ct);

            await ConfirmarAsync(p, creada, cliGuid, corr, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Compensación LiberarCupo tras fallo marketplace reserva {Rev}", p.RevGuid);
            try
            {
                await _inv.LiberarCupoAsync(new LiberarCupoRequest
                {
                    HorGuid = p.HorGuid.ToString("D"),
                    CantidadPersonas = totalPersonas,
                    ReservaGuid = p.RevGuid.ToString("D"),
                }, cancellationToken: ct);
            }
            catch (Exception libEx)
            {
                _logger.LogError(libEx, "No se pudo liberar cupo tras error marketplace {Rev}", p.RevGuid);
            }

            await RechazarAsync(p, ex.Message, corr, ct);
        }
    }

    private async Task ConfirmarAsync(
        MarketplaceReservaSolicitadaPayload p,
        ReservaDetalladaDto creada,
        Guid cliGuid,
        string correlationId,
        CancellationToken ct)
    {
        var payload = EventEnvelope<MarketplaceReservaConfirmadaPayload>.Create(
            EventTypes.MarketplaceReservaConfirmada,
            new MarketplaceReservaConfirmadaPayload
            {
                SeguimientoId = p.SeguimientoId,
                RevGuid = creada.RevGuid,
                RevCodigo = creada.RevCodigo,
                CliGuid = cliGuid,
                AtGuid = p.AtGuid,
                HorGuid = p.HorGuid,
                Total = creada.Total,
                Estado = "P",
            },
            correlationId);

        _db.OutboxEvents.Add(new OutboxEventEntity
        {
            ObGuid = Guid.NewGuid(),
            RoutingKey = EventTypes.MarketplaceReservaConfirmada,
            PayloadJson = payload.ToJson(),
            CorrelationId = correlationId,
            CreatedUtc = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
        await _seguimiento.ActualizarConfirmadaAsync(p.SeguimientoId, creada.RevGuid, creada.RevCodigo, ct);
    }

    private async Task RechazarAsync(
        MarketplaceReservaSolicitadaPayload p,
        string motivo,
        string correlationId,
        CancellationToken ct)
    {
        var payload = EventEnvelope<MarketplaceReservaRechazadaPayload>.Create(
            EventTypes.MarketplaceReservaRechazada,
            new MarketplaceReservaRechazadaPayload
            {
                SeguimientoId = p.SeguimientoId,
                RevGuid = p.RevGuid,
                Motivo = motivo,
            },
            correlationId);

        _db.OutboxEvents.Add(new OutboxEventEntity
        {
            ObGuid = Guid.NewGuid(),
            RoutingKey = EventTypes.MarketplaceReservaRechazada,
            PayloadJson = payload.ToJson(),
            CorrelationId = correlationId,
            CreatedUtc = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync(ct);
        await _seguimiento.ActualizarRechazadaAsync(p.SeguimientoId, motivo, ct);
    }

    private async Task<Guid> ResolverCliGuidAsync(MarketplaceReservaSolicitadaPayload p, CancellationToken ct)
    {
        if (p.CliGuid.HasValue && p.CliGuid.Value != Guid.Empty)
            return p.CliGuid.Value;

        var inv = p.ClienteInvitado
            ?? throw new InvalidOperationException("Debe enviar cli_guid o cliente_invitado.");

        var existente = await _clientes.ObtenerActivoPorNumeroIdentificacionAsync(inv.NumeroIdentificacion.Trim(), ct);
        if (existente is not null)
            return existente.CliGuid;

        var cliGuid = Guid.NewGuid();
        await _clientes.CrearAsync(new CrearClienteInternoDto
        {
            CliGuid = cliGuid,
            TipoIdentificacion = inv.TipoIdentificacion.Trim(),
            NumeroIdentificacion = inv.NumeroIdentificacion.Trim(),
            Nombres = inv.Nombres?.Trim(),
            Apellidos = inv.Apellidos?.Trim(),
            Correo = inv.Correo.Trim(),
            Telefono = inv.Telefono?.Trim(),
            Direccion = inv.Direccion?.Trim(),
            CreadoPor = p.UsuarioAccion,
            IpCreador = p.IpAccion,
        }, ct);
        return cliGuid;
    }

    private async Task<ReservaPrepResult> PrepararLineasAsync(MarketplaceReservaSolicitadaPayload p, CancellationToken ct)
    {
        if (p.Lineas.Count == 0)
            throw new InvalidOperationException("Debe incluir al menos una línea.");

        var hor = await _inv.ObtenerHorarioParaReservaAsync(new ObtenerHorarioParaReservaRequest
        {
            HorGuid = p.HorGuid.ToString("D"),
            AtGuid = p.AtGuid.ToString("D"),
            FechaVisita = p.FechaVisita?.Trim() ?? string.Empty,
        }, cancellationToken: ct);

        if (!hor.Ok)
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(hor.Mensaje) ? "Horario no válido." : hor.Mensaje);

        var lineas = new List<CrearReservaLineaInternaDto>();
        decimal subtotal = 0;

        foreach (var ln in p.Lineas)
        {
            if (ln.Cantidad <= 0)
                throw new InvalidOperationException("Cantidad inválida.");

            var pr = await _inv.GetTicketPrecioAsync(new GetTicketPrecioRequest { TckGuid = ln.TckGuid.ToString("D") }, cancellationToken: ct);
            if (!pr.Ok)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(pr.Mensaje) ? "Ticket no encontrado." : pr.Mensaje);

            var pu = (decimal)pr.Precio;
            var subL = pu * ln.Cantidad;
            subtotal += subL;
            lineas.Add(new CrearReservaLineaInternaDto
            {
                TckGuid = ln.TckGuid,
                Cantidad = ln.Cantidad,
                PrecioUnit = pu,
                SubtotalLinea = subL,
                TipoParticipante = pr.TipoParticipante ?? string.Empty,
            });
        }

        var iva = Math.Round(subtotal * 0.15m, 2);
        return new ReservaPrepResult(
            lineas,
            subtotal,
            iva,
            subtotal + iva,
            hor.AtraccionNombre ?? string.Empty,
            hor.HorFecha ?? string.Empty,
            hor.HorHoraInicio ?? string.Empty,
            hor.HorHoraFin ?? string.Empty);
    }

    private sealed record ReservaPrepResult(
        IReadOnlyList<CrearReservaLineaInternaDto> Lineas,
        decimal Subtotal,
        decimal Iva,
        decimal Total,
        string AtraccionNombre,
        string HorFecha,
        string HorHoraInicio,
        string HorHoraFin);
}
