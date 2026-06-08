using System.Globalization;
using System.Net.Mail;
using Atracciones.Contracts.Clientes.V1;
using Atracciones.Contracts.Facturacion.V1;
using Atracciones.Contracts.Inventario.V1;
using Atracciones.Contracts.Reservas.V1;
using Atracciones.MsOrquestador.Business.Exceptions;
using Atracciones.MsOrquestador.Business.Integration;
using Atracciones.MsOrquestador.Business.Models;
using Atracciones.MsOrquestador.DataManagement.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Atracciones.MsOrquestador.Business.Services;

public sealed class ReservaOrquestacionAppService : IReservaOrquestacionService
{
    private readonly ClienteService.ClienteServiceClient _cli;
    private readonly AtraccionInventarioService.AtraccionInventarioServiceClient _inv;
    private readonly ReservaService.ReservaServiceClient _res;
    private readonly FacturaService.FacturaServiceClient _fac;
    private readonly AuditoriaBestEffortPublisher _audit;
    private readonly ISagaRepository _saga;
    private readonly ILogger<ReservaOrquestacionAppService> _logger;

    public ReservaOrquestacionAppService(
        ClienteService.ClienteServiceClient cli,
        AtraccionInventarioService.AtraccionInventarioServiceClient inv,
        ReservaService.ReservaServiceClient res,
        FacturaService.FacturaServiceClient fac,
        AuditoriaBestEffortPublisher audit,
        ISagaRepository saga,
        ILogger<ReservaOrquestacionAppService> logger)
    {
        _cli = cli;
        _inv = inv;
        _res = res;
        _fac = fac;
        _audit = audit;
        _saga = saga;
        _logger = logger;
    }

    public Task<ReservaResponseDto> CrearReservaAsync(
        CrearReservaOrquestadorDto request,
        Guid? usuGuid,
        string? authorizationBearer,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default) =>
        CrearReservaPendienteAsync(request, usuGuid, authorizationBearer, usuarioAccion, ip, correlationId, ct);

    public async Task<ReservaResponseDto> CrearReservaPendienteAsync(
        CrearReservaOrquestadorDto request,
        Guid? usuGuid,
        string? authorizationBearer,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default)
    {
        var revGuid = Guid.NewGuid();
        var prep = await PrepararReservaAsync(request, usuGuid, authorizationBearer, usuarioAccion, ip, revGuid, ct);

        var sagaId = await _saga.IniciarSagaAsync("CREAR_RESERVA", correlationId, ct);
        var sagaTerminal = false;
        var cupoReservado = false;

        try
        {
            var cupo = await _inv.ValidarYReservarCupoAsync(new ValidarYReservarCupoRequest
            {
                HorGuid = prep.Request.HorGuid.ToString("D"),
                CantidadPersonas = prep.TotalPersonas,
                ReservaGuid = prep.RevGuid.ToString("D"),
            }, cancellationToken: ct);

            if (!cupo.Ok)
            {
                await _saga.RegistrarPasoAsync(sagaId, "CUPO", "FALLIDO", null, cupo.Mensaje, cupo.Mensaje, ct);
                throw new ConflictOrchestadorException(string.IsNullOrWhiteSpace(cupo.Mensaje) ? "Sin cupos suficientes." : cupo.Mensaje);
            }

            cupoReservado = true;
            await _saga.RegistrarPasoAsync(sagaId, "CUPO", "OK", null, cupo.CuposDisponiblesTrasOperacion.ToString(), null, ct);

            var crea = new CrearReservaPendienteRequest
            {
                RevGuid = prep.RevGuid.ToString("D"),
                CliGuid = prep.CliGuid.ToString("D"),
                AtGuid = prep.Request.AtGuid.ToString("D"),
                HorGuid = prep.Request.HorGuid.ToString("D"),
                Subtotal = (double)prep.Subtotal,
                ValorIva = (double)prep.Iva,
                Total = (double)prep.Total,
                OrigenCanal = prep.Request.OrigenCanal ?? "",
                UsuarioIngreso = usuarioAccion,
                IpIngreso = ip,
                AtraccionNombreSnap = prep.AtraccionNombre,
                HorFechaSnap = prep.HorFecha,
                HorHoraInicioSnap = prep.HorHoraInicio,
                HorHoraFinSnap = prep.HorHoraFin,
            };
            foreach (var lg in prep.LineasGrpc)
                crea.Lineas.Add(lg);

            ReservaReply creada;
            try
            {
                creada = await _res.CrearReservaPendienteAsync(crea, cancellationToken: ct);
                await _saga.RegistrarPasoAsync(sagaId, "RESERVA_DB", "OK", null, creada.RevGuid, null, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Compensación LiberarCupo tras fallo CrearReservaPendiente");
                await _inv.LiberarCupoAsync(new LiberarCupoRequest
                {
                    HorGuid = prep.Request.HorGuid.ToString("D"),
                    CantidadPersonas = prep.TotalPersonas,
                    ReservaGuid = prep.RevGuid.ToString("D"),
                }, cancellationToken: ct);
                await _saga.CompletarSagaAsync(sagaId, "COMPENSADA", ct);
                sagaTerminal = true;
                throw;
            }

            await _saga.CompletarSagaAsync(sagaId, "COMPLETADA", ct);
            sagaTerminal = true;

            _audit.Registrar("RESERVA_CREADA", correlationId,
                new { rev_guid = prep.RevGuid.ToString("D"), rev_codigo = creada.RevCodigo, estado = "P" });

            return MapReserva(creada);
        }
        catch (Exception ex) when (ex is not ValidationOrchestadorException && ex is not ConflictOrchestadorException && ex is not NotFoundOrchestadorException)
        {
            if (cupoReservado && !sagaTerminal)
            {
                try
                {
                    await _inv.LiberarCupoAsync(new LiberarCupoRequest
                    {
                        HorGuid = prep.Request.HorGuid.ToString("D"),
                        CantidadPersonas = prep.TotalPersonas,
                        ReservaGuid = prep.RevGuid.ToString("D"),
                    }, cancellationToken: ct);
                }
                catch (Exception libEx)
                {
                    _logger.LogError(libEx, "No se pudo liberar cupo tras error en CrearReservaPendiente {Rev}", prep.RevGuid);
                }
            }

            if (!sagaTerminal)
                await _saga.CompletarSagaAsync(sagaId, "ERROR", ct);
            throw;
        }
    }

    public async Task<(PayPalCheckoutPayload Payload, decimal Total, string Moneda)> PrepararCheckoutPayPalAsync(
        CrearReservaOrquestadorDto request,
        Guid? usuGuid,
        string? authorizationBearer,
        string usuarioAccion,
        string ip,
        CancellationToken ct = default)
    {
        var revGuid = Guid.NewGuid();
        var prep = await PrepararReservaAsync(request, usuGuid, authorizationBearer, usuarioAccion, ip, revGuid, ct);
        var payload = new PayPalCheckoutPayload
        {
            RevGuid = revGuid,
            CliGuid = prep.CliGuid,
            Reserva = request,
        };
        return (payload, prep.Total, "USD");
    }

    public async Task<ReservaResponseDto> CotizarReservaAsync(
        CrearReservaOrquestadorDto request,
        Guid? usuGuid,
        string? authorizationBearer,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default)
    {
        var prep = await PrepararReservaAsync(request, usuGuid, authorizationBearer, usuarioAccion, ip, Guid.NewGuid(), ct);
        return MapCotizacion(prep);
    }

    public async Task<FacturaStubResponseDto> MaterializarReservaTrasPagoCapturadoAsync(
        PayPalCheckoutPayload checkout,
        ConfirmarPagoOrquestadorDto facturacion,
        decimal montoCapturado,
        string monedaCapturada,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default)
    {
        ValidarConfirmar(facturacion);

        var prep = await PrepararReservaAsync(
            checkout.Reserva,
            null,
            null,
            usuarioAccion,
            ip,
            checkout.RevGuid,
            ct,
            cliGuidFijo: checkout.CliGuid);

        var moneda = string.IsNullOrWhiteSpace(monedaCapturada) ? "USD" : monedaCapturada.Trim();
        if (Math.Abs(montoCapturado - prep.Total) > 0.02m)
            throw new ConflictOrchestadorException("El monto capturado no coincide con el total de la reserva.");

        var sagaId = await _saga.IniciarSagaAsync("RESERVA_POST_PAGO_PAYPAL", correlationId, ct);
        var sagaTerminal = false;
        var cupoReservado = false;

        try
        {
            var cupo = await _inv.ValidarYReservarCupoAsync(new ValidarYReservarCupoRequest
            {
                HorGuid = prep.Request.HorGuid.ToString("D"),
                CantidadPersonas = prep.TotalPersonas,
                ReservaGuid = prep.RevGuid.ToString("D"),
            }, cancellationToken: ct);

            if (!cupo.Ok)
            {
                await _saga.RegistrarPasoAsync(sagaId, "CUPO", "FALLIDO", null, cupo.Mensaje, cupo.Mensaje, ct);
                throw new ConflictOrchestadorException(string.IsNullOrWhiteSpace(cupo.Mensaje) ? "Sin cupos suficientes." : cupo.Mensaje);
            }

            cupoReservado = true;
            await _saga.RegistrarPasoAsync(sagaId, "CUPO", "OK", null, cupo.CuposDisponiblesTrasOperacion.ToString(), null, ct);

            var crea = new CrearReservaPendienteRequest
            {
                RevGuid = prep.RevGuid.ToString("D"),
                CliGuid = prep.CliGuid.ToString("D"),
                AtGuid = prep.Request.AtGuid.ToString("D"),
                HorGuid = prep.Request.HorGuid.ToString("D"),
                Subtotal = (double)prep.Subtotal,
                ValorIva = (double)prep.Iva,
                Total = (double)prep.Total,
                OrigenCanal = prep.Request.OrigenCanal ?? "",
                UsuarioIngreso = usuarioAccion,
                IpIngreso = ip,
                AtraccionNombreSnap = prep.AtraccionNombre,
                HorFechaSnap = prep.HorFecha,
                HorHoraInicioSnap = prep.HorHoraInicio,
                HorHoraFinSnap = prep.HorHoraFin,
            };
            foreach (var lg in prep.LineasGrpc)
                crea.Lineas.Add(lg);

            ReservaReply creada;
            try
            {
                creada = await _res.CrearReservaPendienteAsync(crea, cancellationToken: ct);
                await _saga.RegistrarPasoAsync(sagaId, "RESERVA_DB", "OK", null, creada.RevGuid, null, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Compensación LiberarCupo tras fallo CrearReservaPendiente (post-pago)");
                await _inv.LiberarCupoAsync(new LiberarCupoRequest
                {
                    HorGuid = prep.Request.HorGuid.ToString("D"),
                    CantidadPersonas = prep.TotalPersonas,
                    ReservaGuid = prep.RevGuid.ToString("D"),
                }, cancellationToken: ct);
                await _saga.CompletarSagaAsync(sagaId, "COMPENSADA", ct);
                sagaTerminal = true;
                throw;
            }

            var nombreFull = string.Join(' ',
                new[] { facturacion.NombreReceptor.Trim(), facturacion.ApellidoReceptor?.Trim() }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            var confirmada = await _res.ConfirmarReservaPagadaAsync(new ConfirmarReservaPagadaRequest
            {
                RevGuid = prep.RevGuid.ToString("D"),
                UsuarioAccion = usuarioAccion,
                IpAccion = ip,
                NombreReceptor = nombreFull,
                CorreoReceptor = facturacion.CorreoReceptor.Trim(),
                TelefonoReceptor = facturacion.TelefonoReceptor?.Trim() ?? string.Empty,
            }, cancellationToken: ct);

            await _saga.RegistrarPasoAsync(sagaId, "CONFIRMAR_DB", "OK", null, confirmada.RevGuid, null, ct);

            try
            {
                var emit = await _fac.EmitirFacturaAsync(new EmitirFacturaRequest
                {
                    RevGuid = prep.RevGuid.ToString("D"),
                    CliGuid = prep.CliGuid.ToString("D"),
                    Datos = new EmitirFacturaDatos
                    {
                        NombreReceptor = nombreFull,
                        CorreoReceptor = facturacion.CorreoReceptor.Trim(),
                        TelefonoReceptor = facturacion.TelefonoReceptor?.Trim() ?? string.Empty,
                    },
                    Total = confirmada.Total,
                    Moneda = string.IsNullOrWhiteSpace(confirmada.Moneda) ? moneda : confirmada.Moneda,
                    RevCodigoSnap = confirmada.RevCodigo ?? string.Empty,
                    UsuarioEmision = usuarioAccion,
                    IpEmision = ip,
                }, cancellationToken: ct);

                await _saga.RegistrarPasoAsync(sagaId, "FACTURA", "OK", null, emit.FacGuid, null, ct);
                await _saga.CompletarSagaAsync(sagaId, "COMPLETADA", ct);
                sagaTerminal = true;

                _audit.Registrar("RESERVA_CREADA", correlationId,
                    new { rev_guid = prep.RevGuid.ToString("D"), rev_codigo = confirmada.RevCodigo, saga_id = sagaId.ToString() });
                _audit.Registrar("PAGO_CONFIRMADO", correlationId,
                    new { rev_guid = prep.RevGuid.ToString("D"), fac_guid = emit.FacGuid, fac_numero = emit.FacNumero });

                var fechaEmision = DateTime.TryParse(emit.FechaEmisionUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var fe)
                    ? fe
                    : DateTime.UtcNow;

                return new FacturaStubResponseDto
                {
                    RevGuid = prep.RevGuid.ToString("D"),
                    FacGuid = emit.FacGuid,
                    FacNumero = emit.FacNumero,
                    RevCodigo = string.IsNullOrWhiteSpace(emit.RevCodigoSnap)
                        ? (confirmada.RevCodigo ?? string.Empty)
                        : emit.RevCodigoSnap,
                    Total = (decimal)emit.Total,
                    Moneda = emit.Moneda,
                    FechaEmision = fechaEmision,
                    Estado = emit.Estado,
                    NombreReceptor = string.IsNullOrWhiteSpace(emit.NombreReceptor) ? nombreFull : emit.NombreReceptor,
                    CorreoReceptor = facturacion.CorreoReceptor.Trim(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Compensación tras fallo EmitirFactura (post-pago)");
                await _res.AnularReservaAsync(new AnularReservaRequest
                {
                    RevGuid = prep.RevGuid.ToString("D"),
                    Motivo = "Revertir confirmación: fallo al emitir factura.",
                    UsuarioAccion = usuarioAccion,
                    IpAccion = ip,
                }, cancellationToken: ct);

                await _inv.LiberarCupoAsync(new LiberarCupoRequest
                {
                    HorGuid = prep.Request.HorGuid.ToString("D"),
                    CantidadPersonas = prep.TotalPersonas,
                    ReservaGuid = prep.RevGuid.ToString("D"),
                }, cancellationToken: ct);

                await _saga.CompletarSagaAsync(sagaId, "COMPENSADA", ct);
                sagaTerminal = true;
                _audit.Registrar("PAGO_COMPENSADO", correlationId,
                    new { rev_guid = prep.RevGuid.ToString("D"), error = ex.Message });
                throw new ConflictOrchestadorException(
                    "El pago fue recibido pero no se pudo completar la reserva. Contacte soporte; se intentó revertir cupo y reserva.");
            }
        }
        catch (Exception ex) when (ex is not ValidationOrchestadorException && ex is not ConflictOrchestadorException && ex is not NotFoundOrchestadorException)
        {
            if (cupoReservado && !sagaTerminal)
            {
                try
                {
                    await _inv.LiberarCupoAsync(new LiberarCupoRequest
                    {
                        HorGuid = prep.Request.HorGuid.ToString("D"),
                        CantidadPersonas = prep.TotalPersonas,
                        ReservaGuid = prep.RevGuid.ToString("D"),
                    }, cancellationToken: ct);
                }
                catch (Exception libEx)
                {
                    _logger.LogError(libEx, "No se pudo liberar cupo tras error post-pago {Rev}", prep.RevGuid);
                }
            }

            if (!sagaTerminal)
                await _saga.CompletarSagaAsync(sagaId, "ERROR", ct);
            _logger.LogError(ex, "Error en MaterializarReservaTrasPagoCapturado");
            throw;
        }
    }

    private sealed record ReservaPreparada(
        Guid CliGuid,
        Guid RevGuid,
        CrearReservaOrquestadorDto Request,
        IList<LineaDetalleReserva> LineasGrpc,
        decimal Subtotal,
        decimal Iva,
        decimal Total,
        int TotalPersonas,
        string AtraccionNombre,
        string HorFecha,
        string HorHoraInicio,
        string HorHoraFin);

    private async Task<ReservaPreparada> PrepararReservaAsync(
        CrearReservaOrquestadorDto request,
        Guid? usuGuid,
        string? authorizationBearer,
        string usuarioAccion,
        string ip,
        Guid revGuid,
        CancellationToken ct,
        Guid? cliGuidFijo = null)
    {
        if (request.Lineas.Count == 0)
            throw new ValidationOrchestadorException(new[] { "Debe incluir al menos una línea de ticket." });

        Guid cliGuid;
        if (usuGuid.HasValue)
        {
            cliGuid = cliGuidFijo ?? await ResolverCliGuidAsync(usuGuid.Value, authorizationBearer, ct);
        }
        else
        {
            if (request.ClienteInvitado is null)
                throw new ValidationOrchestadorException(new[] { "Debe enviar cliente_invitado o autenticarse con JWT." });

            cliGuid = cliGuidFijo ?? await ResolverCliGuidInvitadoAsync(request.ClienteInvitado, usuarioAccion, ip, ct);
        }

        var hor = await _inv.ObtenerHorarioParaReservaAsync(new ObtenerHorarioParaReservaRequest
        {
            HorGuid = request.HorGuid.ToString("D"),
            AtGuid = request.AtGuid.ToString("D"),
            FechaVisita = request.FechaVisita?.Trim() ?? string.Empty,
        }, cancellationToken: ct);

        if (!hor.Ok)
            throw new ConflictOrchestadorException(string.IsNullOrWhiteSpace(hor.Mensaje) ? "Horario no válido." : hor.Mensaje);

        var lineasGrpc = new List<LineaDetalleReserva>();
        decimal subtotal = 0;
        foreach (var ln in request.Lineas)
        {
            if (ln.Cantidad <= 0)
                throw new ValidationOrchestadorException(new[] { "La cantidad debe ser mayor a 0." });

            var pr = await _inv.GetTicketPrecioAsync(new GetTicketPrecioRequest { TckGuid = ln.TckGuid.ToString("D") }, cancellationToken: ct);
            if (!pr.Ok)
                throw new NotFoundOrchestadorException(string.IsNullOrWhiteSpace(pr.Mensaje) ? "Ticket no encontrado." : pr.Mensaje);

            if (!string.Equals(pr.AtGuid, request.AtGuid.ToString("D"), StringComparison.OrdinalIgnoreCase))
                throw new ConflictOrchestadorException("Un ticket de las líneas no pertenece a la atracción indicada.");

            if (!string.IsNullOrEmpty(hor.TckGuid) &&
                !string.Equals(ln.TckGuid.ToString("D"), hor.TckGuid, StringComparison.OrdinalIgnoreCase))
                throw new ConflictOrchestadorException(
                    $"El ticket '{ln.TckGuid}' no corresponde al horario seleccionado. El horario solo acepta el ticket '{hor.TckGuid}'.");

            var pu = (decimal)pr.Precio;
            var subL = pu * ln.Cantidad;
            subtotal += subL;
            lineasGrpc.Add(new LineaDetalleReserva
            {
                TckGuid = ln.TckGuid.ToString("D"),
                Cantidad = ln.Cantidad,
                PrecioUnit = (double)pu,
                SubtotalLinea = (double)subL,
                TipoParticipante = pr.TipoParticipante ?? "",
            });
        }

        var iva = Math.Round(subtotal * 0.15m, 2);
        var total = subtotal + iva;
        var totalPersonas = request.Lineas.Sum(l => l.Cantidad);

        return new ReservaPreparada(
            cliGuid,
            revGuid,
            request,
            lineasGrpc,
            subtotal,
            iva,
            total,
            totalPersonas,
            hor.AtraccionNombre,
            hor.HorFecha,
            hor.HorHoraInicio,
            hor.HorHoraFin ?? "");
    }

    private static ReservaResponseDto MapCotizacion(ReservaPreparada prep) =>
        new()
        {
            RevGuid = prep.RevGuid.ToString("D"),
            RevCodigo = string.Empty,
            HorFecha = prep.HorFecha,
            HorHoraInicio = prep.HorHoraInicio,
            HorHoraFin = string.IsNullOrWhiteSpace(prep.HorHoraFin) ? null : prep.HorHoraFin,
            AtraccionNombre = prep.AtraccionNombre,
            RevSubtotal = prep.Subtotal,
            RevValorIva = prep.Iva,
            RevTotal = prep.Total,
            Moneda = "USD",
            RevEstado = "COTIZACION",
            RevFechaReservaUtc = DateTime.UtcNow,
            Detalle = prep.LineasGrpc.Select(l => new ReservaDetalleResponseDto
            {
                TckTipoParticipante = l.TipoParticipante,
                Cantidad = l.Cantidad,
                PrecioUnit = (decimal)l.PrecioUnit,
                Subtotal = (decimal)l.SubtotalLinea,
            }).ToList(),
        };

    public async Task<FacturaStubResponseDto> CompletarPagoReservaYFacturaAsync(
        Guid revGuid,
        ConfirmarPagoOrquestadorDto request,
        string usuarioAccion,
        string ip,
        string correlationId,
        bool compensarSiFallaFactura,
        CancellationToken ct = default)
    {
        ValidarConfirmar(request);

        var sagaId = await _saga.IniciarSagaAsync("CONFIRMAR_PAGO_PAYPAL", correlationId, ct);
        var sagaTerminal = false;
        try
        {
            var pendiente = await _res.ObtenerReservaAsync(new ObtenerReservaRequest { RevGuid = revGuid.ToString("D") }, cancellationToken: ct);
            if (pendiente.Estado != "P")
                throw new ConflictOrchestadorException("La reserva no está pendiente de pago o ya fue procesada.");

            var horGuid = pendiente.HorGuid;
            var totalPersonas = pendiente.Detalle.Sum(d => d.Cantidad);
            var cliGuidStr = pendiente.CliGuid;

            var nombreFull = string.Join(' ',
                new[] { request.NombreReceptor.Trim(), request.ApellidoReceptor?.Trim() }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            var confirmada = await _res.ConfirmarReservaPagadaAsync(new ConfirmarReservaPagadaRequest
            {
                RevGuid = revGuid.ToString("D"),
                UsuarioAccion = usuarioAccion,
                IpAccion = ip,
                NombreReceptor = nombreFull,
                CorreoReceptor = request.CorreoReceptor.Trim(),
                TelefonoReceptor = request.TelefonoReceptor?.Trim() ?? string.Empty,
            }, cancellationToken: ct);

            await _saga.RegistrarPasoAsync(sagaId, "CONFIRMAR_DB", "OK", null, confirmada.RevGuid, null, ct);

            try
            {
                var emit = await _fac.EmitirFacturaAsync(new EmitirFacturaRequest
                {
                    RevGuid = revGuid.ToString("D"),
                    CliGuid = cliGuidStr,
                    Datos = new EmitirFacturaDatos
                    {
                        NombreReceptor = nombreFull,
                        CorreoReceptor = request.CorreoReceptor.Trim(),
                        TelefonoReceptor = request.TelefonoReceptor?.Trim() ?? string.Empty,
                    },
                    Total = confirmada.Total,
                    Moneda = string.IsNullOrWhiteSpace(confirmada.Moneda) ? "USD" : confirmada.Moneda,
                    RevCodigoSnap = confirmada.RevCodigo ?? string.Empty,
                    UsuarioEmision = usuarioAccion,
                    IpEmision = ip,
                }, cancellationToken: ct);

                await _saga.RegistrarPasoAsync(sagaId, "FACTURA", "OK", null, emit.FacGuid, null, ct);
                await _saga.CompletarSagaAsync(sagaId, "COMPLETADA", ct);
                sagaTerminal = true;

                _audit.Registrar("PAGO_CONFIRMADO", correlationId,
                    new { rev_guid = revGuid.ToString("D"), fac_guid = emit.FacGuid, fac_numero = emit.FacNumero });

                var fechaEmision = DateTime.TryParse(emit.FechaEmisionUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var fe)
                    ? fe
                    : DateTime.UtcNow;

                return new FacturaStubResponseDto
                {
                    RevGuid = revGuid.ToString("D"),
                    FacGuid = emit.FacGuid,
                    FacNumero = emit.FacNumero,
                    RevCodigo = string.IsNullOrWhiteSpace(emit.RevCodigoSnap)
                        ? (confirmada.RevCodigo ?? string.Empty)
                        : emit.RevCodigoSnap,
                    Total = (decimal)emit.Total,
                    Moneda = emit.Moneda,
                    FechaEmision = fechaEmision,
                    Estado = emit.Estado,
                    NombreReceptor = string.IsNullOrWhiteSpace(emit.NombreReceptor) ? nombreFull : emit.NombreReceptor,
                    CorreoReceptor = request.CorreoReceptor.Trim(),
                };
            }
            catch (Exception ex)
            {
                if (compensarSiFallaFactura)
                {
                    _logger.LogWarning(ex, "Compensación tras fallo EmitirFactura");
                    await _res.AnularReservaAsync(new AnularReservaRequest
                    {
                        RevGuid = revGuid.ToString("D"),
                        Motivo = "Revertir confirmación: fallo al emitir factura.",
                        UsuarioAccion = usuarioAccion,
                        IpAccion = ip,
                    }, cancellationToken: ct);

                    await _inv.LiberarCupoAsync(new LiberarCupoRequest
                    {
                        HorGuid = horGuid,
                        CantidadPersonas = totalPersonas,
                        ReservaGuid = revGuid.ToString("D"),
                    }, cancellationToken: ct);

                    await _saga.RegistrarPasoAsync(sagaId, "COMPENSACION", "OK", null, "Anular+LiberarCupo", null, ct);
                    await _saga.CompletarSagaAsync(sagaId, "COMPENSADA", ct);
                    sagaTerminal = true;
                    _audit.Registrar("PAGO_COMPENSADO", correlationId,
                        new { rev_guid = revGuid.ToString("D"), error = ex.Message });
                    throw;
                }

                await _saga.CompletarSagaAsync(sagaId, "FACTURA_ERROR", ct);
                sagaTerminal = true;
                _logger.LogError(ex, "EmitirFactura falló tras pago verificado; la reserva permanece confirmada.");
                _audit.Registrar("FACTURA_POST_PAGO_FALLIDA", correlationId,
                    new { rev_guid = revGuid.ToString("D"), error = ex.Message });
                throw new ConflictOrchestadorException(
                    "El pago quedó registrado pero no se pudo emitir la factura automáticamente. Contacte soporte con su código de reserva.");
            }
        }
        catch (Exception ex)
        {
            if (!sagaTerminal)
                await _saga.CompletarSagaAsync(sagaId, "ERROR", ct);
            _logger.LogWarning(ex, "Fallo CompletarPagoReservaYFactura");
            throw;
        }
    }

    public async Task CancelarReservaAsync(
        Guid revGuid,
        string motivo,
        Guid usuGuidCliente,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default)
    {
        var sagaId = await _saga.IniciarSagaAsync("CANCELAR_RESERVA", correlationId, ct);
        var r = await _res.ObtenerReservaAsync(new ObtenerReservaRequest { RevGuid = revGuid.ToString("D") }, cancellationToken: ct);
        if (!string.Equals(r.CliGuid, usuGuidCliente.ToString("D"), StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenOrchestadorException("No puedes cancelar una reserva que no te pertenece.");

        if (r.Estado != "P" && r.Estado != "A")
            throw new ConflictOrchestadorException("La reserva no se puede cancelar en su estado actual.");

        var totalPersonas = r.Detalle.Sum(d => d.Cantidad);
        var m = string.IsNullOrWhiteSpace(motivo) ? "Cancelada por el cliente." : motivo.Trim();

        await _res.AnularReservaAsync(new AnularReservaRequest
        {
            RevGuid = revGuid.ToString("D"),
            Motivo = m,
            UsuarioAccion = usuarioAccion,
            IpAccion = ip,
        }, cancellationToken: ct);

        await _inv.LiberarCupoAsync(new LiberarCupoRequest
        {
            HorGuid = r.HorGuid,
            CantidadPersonas = totalPersonas,
            ReservaGuid = revGuid.ToString("D"),
        }, cancellationToken: ct);

        await _saga.CompletarSagaAsync(sagaId, "COMPLETADA", ct);
        _audit.Registrar("RESERVA_CANCELADA", correlationId, new { rev_guid = revGuid.ToString("D"), motivo = m });
    }

    public async Task<ReservaResponseDto> ObtenerReservaAsync(Guid revGuid, Guid usuGuidCliente, CancellationToken ct = default)
    {
        var r = await _res.ObtenerReservaAsync(new ObtenerReservaRequest { RevGuid = revGuid.ToString("D") }, cancellationToken: ct);
        if (!string.Equals(r.CliGuid, usuGuidCliente.ToString("D"), StringComparison.OrdinalIgnoreCase))
            throw new ForbiddenOrchestadorException("No tienes acceso a esta reserva.");
        return MapReserva(r);
    }

    public async Task<(IReadOnlyList<ReservaResponseDto> Items, int Total)> ListarMisReservasAsync(
        Guid usuGuidCliente,
        int page,
        int limit,
        CancellationToken ct = default)
    {
        var resp = await _res.ListarMisReservasAsync(new ListarMisReservasRequest
        {
            CliGuid = usuGuidCliente.ToString("D"),
            Page = page,
            Limit = limit,
        }, cancellationToken: ct);

        var items = resp.Items.Select(MapReserva).ToList();
        return (items, resp.TotalFiltrado);
    }

    private async Task<Guid> ResolverCliGuidAsync(
        Guid usuGuid,
        string? authorizationBearer,
        CancellationToken ct)
    {
        var md = AuthMetadata(authorizationBearer);
        try
        {
            await _cli.ObtenerClientePorGuidAsync(
                new ObtenerClienteRequest { CliGuid = usuGuid.ToString("D") },
                md,
                cancellationToken: ct);
            return usuGuid;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new NotFoundOrchestadorException("No existe perfil de cliente para este usuario. Regístrese o complete su perfil.");
        }
    }

    private async Task<Guid> ResolverCliGuidInvitadoAsync(
        ClienteInvitadoOrquestadorDto invitado,
        string usuarioAccion,
        string ip,
        CancellationToken ct)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(invitado.TipoIdentificacion))
            errores.Add("cliente_invitado.tipo_identificacion es obligatorio.");
        if (string.IsNullOrWhiteSpace(invitado.NumeroIdentificacion))
            errores.Add("cliente_invitado.numero_identificacion es obligatorio.");
        if (string.IsNullOrWhiteSpace(invitado.Correo))
            errores.Add("cliente_invitado.correo es obligatorio.");
        if (errores.Count > 0)
            throw new ValidationOrchestadorException(errores);

        try
        {
            var existente = await _cli.ObtenerClientePorNumeroIdentificacionAsync(
                new ObtenerClientePorDocRequest { NumeroIdentificacion = invitado.NumeroIdentificacion.Trim() },
                cancellationToken: ct);
            if (Guid.TryParse(existente.CliGuid, out var g))
                return g;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            // crear nuevo
        }

        var cliGuid = Guid.NewGuid();
        await _cli.CrearClienteAsync(new CrearClienteRequest
        {
            UsuGuid = cliGuid.ToString("D"),
            TipoIdentificacion = invitado.TipoIdentificacion.Trim(),
            NumeroIdentificacion = invitado.NumeroIdentificacion.Trim(),
            Nombres = invitado.Nombres?.Trim() ?? string.Empty,
            Apellidos = invitado.Apellidos?.Trim() ?? string.Empty,
            Correo = invitado.Correo.Trim(),
            Telefono = invitado.Telefono?.Trim() ?? string.Empty,
            Direccion = invitado.Direccion?.Trim() ?? string.Empty,
            CreadoPor = usuarioAccion,
            IpCreador = ip,
        }, cancellationToken: ct);

        return cliGuid;
    }

    private static string MapEstadoPublico(string estado) =>
        estado switch
        {
            "P" => "PENDIENTE",
            "A" => "PAGADA",
            "C" => "CANCELADA",
            "I" => "INACTIVA",
            _ => estado,
        };

    private static void ValidarConfirmar(ConfirmarPagoOrquestadorDto request)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(request.NombreReceptor))
            errores.Add("El nombre del receptor es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.CorreoReceptor))
            errores.Add("El correo del receptor es obligatorio.");
        else
        {
            try
            {
                _ = new MailAddress(request.CorreoReceptor.Trim());
            }
            catch
            {
                errores.Add("El correo del receptor no tiene un formato válido.");
            }
        }

        if (errores.Count > 0)
            throw new ValidationOrchestadorException(errores);
    }

    private static Metadata AuthMetadata(string? bearer)
    {
        var md = new Metadata();
        if (string.IsNullOrWhiteSpace(bearer))
            return md;
        var v = bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? bearer.Trim() : "Bearer " + bearer.Trim();
        md.Add("Authorization", v);
        return md;
    }

    private static ReservaResponseDto MapReserva(ReservaReply r)
    {
        var dto = new ReservaResponseDto
        {
            RevGuid = r.RevGuid,
            AtGuid = r.AtGuid,
            RevCodigo = r.RevCodigo,
            HorFecha = r.HorFechaSnap,
            HorHoraInicio = r.HorHoraInicioSnap,
            HorHoraFin = string.IsNullOrWhiteSpace(r.HorHoraFinSnap) ? null : r.HorHoraFinSnap,
            AtraccionNombre = r.AtraccionNombreSnap,
            RevSubtotal = (decimal)r.Subtotal,
            RevValorIva = (decimal)r.ValorIva,
            RevTotal = (decimal)r.Total,
            Moneda = string.IsNullOrWhiteSpace(r.Moneda) ? "USD" : r.Moneda,
            RevEstado = MapEstadoPublico(r.Estado),
            RevFechaReservaUtc = DateTime.TryParse(r.RevFechaReservaUtc, out var fecha) ? fecha : DateTime.UtcNow,
            Links = new Dictionary<string, string?>(),
        };

        dto.Links["self"] = $"/api/v2/reservas/{r.RevGuid}";
        if (string.Equals(r.Estado, "P", StringComparison.OrdinalIgnoreCase))
            dto.Links["confirmar_pago"] = $"/api/v2/reservas/{r.RevGuid}/pagos/confirmacion";

        foreach (var d in r.Detalle)
        {
            dto.Detalle.Add(new ReservaDetalleResponseDto
            {
                TckTipoParticipante = d.TipoParticipante,
                Cantidad = d.Cantidad,
                PrecioUnit = (decimal)d.PrecioUnit,
                Subtotal = (decimal)d.SubtotalLinea,
            });
        }

        return dto;
    }
}
