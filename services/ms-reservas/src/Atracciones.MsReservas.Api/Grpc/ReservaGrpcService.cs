using Atracciones.Contracts.Reservas.V1;
using Atracciones.MsReservas.Api.EventBus;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Atracciones.MsReservas.DataManagement.Models;
using Grpc.Core;

namespace Atracciones.MsReservas.Api.Grpc;

public sealed class ReservaGrpcService : ReservaService.ReservaServiceBase
{
    private readonly IReservaRepository _repo;
    private readonly ReservaPagadaOutboxPublisher _pagadaPublisher;
    private readonly ILogger<ReservaGrpcService> _logger;

    public ReservaGrpcService(
        IReservaRepository repo,
        ReservaPagadaOutboxPublisher pagadaPublisher,
        ILogger<ReservaGrpcService> logger)
    {
        _repo = repo;
        _pagadaPublisher = pagadaPublisher;
        _logger = logger;
    }

    public override async Task<ReservaReply> CrearReservaPendiente(CrearReservaPendienteRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CliGuid, out var cliGuid)
            || !Guid.TryParse(request.AtGuid, out var atGuid)
            || !Guid.TryParse(request.HorGuid, out var horGuid))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "cli_guid, at_guid u hor_guid inválido."));
        }

        if (request.Lineas.Count == 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Debe incluir al menos una línea."));

        Guid? revPre = null;
        if (!string.IsNullOrWhiteSpace(request.RevGuid) && Guid.TryParse(request.RevGuid, out var rg))
            revPre = rg;

        var lineas = new List<CrearReservaLineaInternaDto>();
        foreach (var ln in request.Lineas)
        {
            if (!Guid.TryParse(ln.TckGuid, out var tckGuid) || ln.Cantidad <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Línea de detalle inválida."));
            lineas.Add(new CrearReservaLineaInternaDto
            {
                TckGuid = tckGuid,
                Cantidad = ln.Cantidad,
                PrecioUnit = (decimal)ln.PrecioUnit,
                SubtotalLinea = (decimal)ln.SubtotalLinea,
                TipoParticipante = ln.TipoParticipante ?? string.Empty,
            });
        }

        try
        {
            var dto = await _repo.CrearPendienteAsync(new CrearReservaInternaDto
            {
                RevGuidPreasignado = revPre,
                CliGuid = cliGuid,
                AtGuid = atGuid,
                HorGuid = horGuid,
                RevCodigo = string.Empty,
                Subtotal = (decimal)request.Subtotal,
                ValorIva = (decimal)request.ValorIva,
                Total = (decimal)request.Total,
                OrigenCanal = string.IsNullOrWhiteSpace(request.OrigenCanal) ? null : request.OrigenCanal.Trim(),
                UsuarioIngreso = string.IsNullOrWhiteSpace(request.UsuarioIngreso) ? "orquestador" : request.UsuarioIngreso.Trim(),
                IpIngreso = string.IsNullOrWhiteSpace(request.IpIngreso) ? "0.0.0.0" : request.IpIngreso.Trim(),
                AtraccionNombreSnap = request.AtraccionNombreSnap ?? string.Empty,
                HorFechaSnap = request.HorFechaSnap ?? string.Empty,
                HorHoraInicioSnap = request.HorHoraInicioSnap ?? string.Empty,
                HorHoraFinSnap = request.HorHoraFinSnap ?? string.Empty,
                Lineas = lineas,
            }, context.CancellationToken);

            return ReservaGrpcMapper.ToReply(dto);
        }
        catch (Exception ex) when (ex is not RpcException)
        {
            _logger.LogError(ex, "CrearReservaPendiente");
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<ReservaReply> ConfirmarReservaPagada(ConfirmarReservaPagadaRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RevGuid, out var revGuid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "rev_guid inválido"));

        try
        {
            var usuario = string.IsNullOrWhiteSpace(request.UsuarioAccion) ? "orquestador" : request.UsuarioAccion.Trim();
            var ip = string.IsNullOrWhiteSpace(request.IpAccion) ? "0.0.0.0" : request.IpAccion.Trim();
            var dto = await _repo.ConfirmarPagadaAsync(revGuid, usuario, ip, context.CancellationToken)
                ?? throw new RpcException(new Status(StatusCode.NotFound, "Reserva no encontrada."));

            var corr = context.RequestHeaders.FirstOrDefault(h =>
                string.Equals(h.Key, "x-correlation-id", StringComparison.OrdinalIgnoreCase))?.Value
                ?? Guid.NewGuid().ToString("D");

            try
            {
                await _pagadaPublisher.TryEnqueueAsync(
                    dto,
                    request.NombreReceptor,
                    request.CorreoReceptor,
                    string.IsNullOrWhiteSpace(request.TelefonoReceptor) ? null : request.TelefonoReceptor,
                    corr,
                    context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo encolar reservas.reserva.pagada para {RevGuid}", revGuid);
            }

            return ReservaGrpcMapper.ToReply(dto);
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<AnularReservaReply> AnularReserva(AnularReservaRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RevGuid, out var revGuid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "rev_guid inválido"));

        try
        {
            var usuario = string.IsNullOrWhiteSpace(request.UsuarioAccion) ? "orquestador" : request.UsuarioAccion.Trim();
            var ip = string.IsNullOrWhiteSpace(request.IpAccion) ? "0.0.0.0" : request.IpAccion.Trim();
            var motivo = string.IsNullOrWhiteSpace(request.Motivo) ? "Anulación." : request.Motivo.Trim();
            var ok = await _repo.AnularAsync(revGuid, motivo, usuario, ip, context.CancellationToken);
            if (!ok)
                throw new RpcException(new Status(StatusCode.NotFound, "Reserva no encontrada."));
            return new AnularReservaReply { Ok = true };
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<ReservaReply> ObtenerReserva(ObtenerReservaRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.RevGuid, out var revGuid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "rev_guid inválido"));

        var dto = await _repo.ObtenerPorGuidAsync(revGuid, context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Reserva no encontrada."));
        return ReservaGrpcMapper.ToReply(dto);
    }

    public override async Task<ListarMisReservasReply> ListarMisReservas(ListarMisReservasRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.CliGuid, out var cliGuid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "cli_guid inválido"));

        var page = request.Page <= 0 ? 1 : request.Page;
        var limit = request.Limit <= 0 ? 10 : request.Limit;

        var (items, total) = await _repo.ListarPorClienteAsync(cliGuid, page, limit, context.CancellationToken);
        var reply = new ListarMisReservasReply { TotalFiltrado = total };
        foreach (var i in items)
            reply.Items.Add(ReservaGrpcMapper.ToReply(i));
        return reply;
    }
}
