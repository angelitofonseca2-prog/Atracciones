using System.Text.Json;
using Atracciones.Contracts.Reservas.V1;
using Atracciones.MsOrquestador.Business.Exceptions;
using Atracciones.MsOrquestador.Business.Integration;
using Atracciones.MsOrquestador.Business.Models;
using Atracciones.MsOrquestador.Business.PayPal;
using Atracciones.MsOrquestador.DataManagement.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Atracciones.MsOrquestador.Business.Services;

public interface IPayPalPagosService
{
    Task<CreatePayPalOrderResult> CrearOrdenAsync(
        Guid revGuid,
        string? revCodigo,
        Guid? usuGuidCliente,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default);

    Task<FacturaStubResponseDto> CapturarYCompletarReservaAsync(
        Guid revGuid,
        string? revCodigo,
        Guid? usuGuidCliente,
        string paypalOrderId,
        ConfirmarPagoOrquestadorDto facturacion,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default);

    Task ProcesarWebhookAsync(string rawBody, CancellationToken ct = default);
}

public sealed record CreatePayPalOrderResult(string PaypalOrderId, string Moneda, decimal Monto);

public sealed class PayPalPagosAppService : IPayPalPagosService
{
    private readonly PayPalApiClient _paypal;
    private readonly IPayPalPaymentRepository _pagos;
    private readonly IReservaOrquestacionService _reservas;
    private readonly ReservaService.ReservaServiceClient _resGrpc;
    private readonly ILogger<PayPalPagosAppService> _logger;

    public PayPalPagosAppService(
        PayPalApiClient paypal,
        IPayPalPaymentRepository pagos,
        IReservaOrquestacionService reservas,
        ReservaService.ReservaServiceClient resGrpc,
        ILogger<PayPalPagosAppService> logger)
    {
        _paypal = paypal;
        _pagos = pagos;
        _reservas = reservas;
        _resGrpc = resGrpc;
        _logger = logger;
    }

    public async Task<CreatePayPalOrderResult> CrearOrdenAsync(
        Guid revGuid,
        string? revCodigo,
        Guid? usuGuidCliente,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default)
    {
        if (!_paypal.IsConfigured)
            throw new ServiceUnavailableOrchestadorException(
                "PayPal no está configurado en el orquestador (PayPal:ClientId / ClientSecret).");

        var pendiente = await ObtenerReservaAutorizadaAsync(revGuid, revCodigo, usuGuidCliente, ct);
        if (pendiente.Estado != "P")
            throw new ConflictOrchestadorException("La reserva no está pendiente de pago.");

        var total = (decimal)pendiente.Total;
        var moneda = string.IsNullOrWhiteSpace(pendiente.Moneda) ? "USD" : pendiente.Moneda.Trim();

        var orderId = await _paypal.CreateOrderAsync(total, moneda, revGuid.ToString("D"), ct);
        await _pagos.InsertAsync(revGuid, orderId, PayPalPaymentEstados.OrderCreated, total, moneda, ct);
        _logger.LogInformation("PayPal orden creada {OrderId} rev {Rev}", orderId, revGuid);
        return new CreatePayPalOrderResult(orderId, moneda, total);
    }

    public async Task<FacturaStubResponseDto> CapturarYCompletarReservaAsync(
        Guid revGuid,
        string? revCodigo,
        Guid? usuGuidCliente,
        string paypalOrderId,
        ConfirmarPagoOrquestadorDto facturacion,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default)
    {
        if (!_paypal.IsConfigured)
            throw new ServiceUnavailableOrchestadorException("PayPal no está configurado.");

        var pendiente = await ObtenerReservaAutorizadaAsync(revGuid, revCodigo, usuGuidCliente, ct);
        if (pendiente.Estado != "P")
            throw new ConflictOrchestadorException("La reserva no está pendiente de pago.");

        var row = await _pagos.GetByPaypalOrderIdAsync(paypalOrderId, ct)
            ?? throw new NotFoundOrchestadorException("No existe una orden PayPal asociada a este pago.");

        if (row.RevGuid != revGuid)
            throw new ForbiddenOrchestadorException("La orden PayPal no corresponde a esta reserva.");

        if (string.Equals(row.EstadoPago, PayPalPaymentEstados.Captured, StringComparison.OrdinalIgnoreCase))
            throw new ConflictOrchestadorException("Esta orden PayPal ya fue capturada.");

        var cap = await _paypal.CaptureOrderAsync(paypalOrderId, ct);

        if (!string.Equals(cap.CustomId, revGuid.ToString("D"), StringComparison.OrdinalIgnoreCase))
            throw new ConflictOrchestadorException("El custom_id de la captura no coincide con la reserva.");

        if (!string.Equals(cap.CurrencyCode, string.IsNullOrWhiteSpace(pendiente.Moneda) ? "USD" : pendiente.Moneda.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ConflictOrchestadorException("La moneda de la captura no coincide con la reserva.");

        var esperado = (decimal)pendiente.Total;
        if (Math.Abs(cap.Amount - esperado) > 0.02m)
            throw new ConflictOrchestadorException("El monto capturado no coincide con el total de la reserva.");

        var dto = facturacion;
        var factura = await _reservas.CompletarPagoReservaYFacturaAsync(
            revGuid,
            dto,
            usuarioAccion,
            ip,
            correlationId,
            compensarSiFallaFactura: false,
            ct);

        await _pagos.UpdateEstadoAsync(row.PayPaymentId, PayPalPaymentEstados.Captured, cap.CaptureId, null, ct);
        return factura;
    }

    public async Task ProcesarWebhookAsync(string rawBody, CancellationToken ct = default)
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(rawBody) ? "{}" : rawBody);
        var root = doc.RootElement;

        if (!root.TryGetProperty("event_type", out var et) || et.GetString() is not { } eventType)
            return;

        if (!string.Equals(eventType, "PAYMENT.CAPTURE.COMPLETED", StringComparison.OrdinalIgnoreCase))
            return;

        if (!root.TryGetProperty("resource", out var resource))
            return;

        var captureId = resource.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        if (string.IsNullOrEmpty(captureId))
            return;

        var prev = await _pagos.GetByPaypalCaptureIdAsync(captureId, ct);
        if (prev is not null && string.Equals(prev.EstadoPago, PayPalPaymentEstados.Captured, StringComparison.OrdinalIgnoreCase))
            return;

        string? orderId = null;
        if (resource.TryGetProperty("supplementary_data", out var sup)
            && sup.TryGetProperty("related_ids", out var rel)
            && rel.TryGetProperty("order_id", out var oid))
            orderId = oid.GetString();

        PayPalPaymentRow? orderRow = null;
        if (!string.IsNullOrEmpty(orderId))
            orderRow = await _pagos.GetByPaypalOrderIdAsync(orderId, ct);

        var customId = resource.TryGetProperty("custom_id", out var c) ? c.GetString() : null;
        if (string.IsNullOrEmpty(customId) || !Guid.TryParse(customId, out var revGuid))
        {
            if (orderRow is not null)
                revGuid = orderRow.RevGuid;
            else
                return;
        }

        decimal amount = 0;
        string currency = "USD";
        if (resource.TryGetProperty("amount", out var amt))
        {
            if (amt.TryGetProperty("value", out var v) && decimal.TryParse(v.GetString(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var dec))
                amount = dec;
            if (amt.TryGetProperty("currency_code", out var cur))
                currency = cur.GetString() ?? "USD";
        }

        ReservaReply pendiente;
        try
        {
            pendiente = await _resGrpc.ObtenerReservaAsync(new ObtenerReservaRequest { RevGuid = revGuid.ToString("D") }, cancellationToken: ct);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning("Webhook PayPal: reserva {Rev} no encontrada", revGuid);
            return;
        }

        if (pendiente.Estado != "P")
            return;

        if (Math.Abs(amount - (decimal)pendiente.Total) > 0.02m
            || !string.Equals(currency, string.IsNullOrWhiteSpace(pendiente.Moneda) ? "USD" : pendiente.Moneda.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Webhook PayPal: monto/moneda no coinciden para {Rev}", revGuid);
            return;
        }

        var (nombre, correo, telefono) = ExtraerPagador(resource);
        if (string.IsNullOrWhiteSpace(correo))
        {
            _logger.LogWarning("Webhook PayPal: sin correo de pagador para completar factura {Rev}", revGuid);
            return;
        }

        var dto = new ConfirmarPagoOrquestadorDto
        {
            NombreReceptor = string.IsNullOrWhiteSpace(nombre) ? "Cliente" : nombre,
            CorreoReceptor = correo.Trim(),
            TelefonoReceptor = telefono,
        };

        try
        {
            await _reservas.CompletarPagoReservaYFacturaAsync(
                revGuid,
                dto,
                usuarioAccion: "paypal-webhook",
                ip: "0.0.0.0",
                correlationId: captureId,
                compensarSiFallaFactura: false,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Webhook PayPal: no se pudo completar reserva {Rev}", revGuid);
            return;
        }

        if (orderRow is not null)
            await _pagos.UpdateEstadoAsync(orderRow.PayPaymentId, PayPalPaymentEstados.Captured, captureId, null, ct);
    }

    private static (string? nombre, string? correo, string? telefono) ExtraerPagador(JsonElement resource)
    {
        string? correo = null;
        string? nombre = null;
        string? telefono = null;
        if (resource.TryGetProperty("payer", out var payer))
        {
            if (payer.TryGetProperty("email_address", out var em))
                correo = em.GetString();
            if (payer.TryGetProperty("name", out var nm))
            {
                var given = nm.TryGetProperty("given_name", out var g) ? g.GetString() : null;
                var surname = nm.TryGetProperty("surname", out var s) ? s.GetString() : null;
                nombre = string.Join(' ', new[] { given, surname }.Where(x => !string.IsNullOrWhiteSpace(x)));
            }
            if (payer.TryGetProperty("phone", out var ph) && ph.TryGetProperty("phone_number", out var pn))
                telefono = pn.GetString();
        }

        return (nombre, correo, telefono);
    }

    private async Task<ReservaReply> ObtenerReservaAutorizadaAsync(
        Guid revGuid,
        string? revCodigo,
        Guid? usuGuidCliente,
        CancellationToken ct)
    {
        ReservaReply r;
        try
        {
            r = await _resGrpc.ObtenerReservaAsync(new ObtenerReservaRequest { RevGuid = revGuid.ToString("D") }, cancellationToken: ct);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new NotFoundOrchestadorException("Reserva no encontrada.");
        }

        if (usuGuidCliente.HasValue)
        {
            if (!string.Equals(r.CliGuid, usuGuidCliente.Value.ToString("D"), StringComparison.OrdinalIgnoreCase))
                throw new ForbiddenOrchestadorException("No tienes permiso para pagar esta reserva.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(revCodigo)
                || !string.Equals(r.RevCodigo?.Trim(), revCodigo.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new ValidationOrchestadorException(new[]
                {
                    "Debe enviar rev_codigo (código de reserva) cuando no hay sesión iniciada.",
                });
        }

        return r;
    }
}
