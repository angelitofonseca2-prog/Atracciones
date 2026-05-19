using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atracciones.MsOrquestador.Api.Models.Common;
using Atracciones.MsOrquestador.Api.Models.Pagos;
using Atracciones.MsOrquestador.Business.Exceptions;
using Atracciones.MsOrquestador.Business.Integration;
using Atracciones.MsOrquestador.Business.Models;
using Atracciones.MsOrquestador.Business.Services;
using Atracciones.MsOrquestador.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsOrquestador.Api.Controllers;

[ApiController]
[Route("api/v1/pagos/paypal")]
public sealed class PayPalPagosController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly IPayPalPagosService _pagos;
    private readonly PayPalApiClient _paypal;
    private readonly IIdempotencyRepository _idem;

    public PayPalPagosController(IPayPalPagosService pagos, PayPalApiClient paypal, IIdempotencyRepository idem)
    {
        _pagos = pagos;
        _paypal = paypal;
        _idem = idem;
    }

    private Guid UsuGuidActual
    {
        get
        {
            var claim = User.FindFirstValue("usu_guid");
            if (!Guid.TryParse(claim, out var g))
                throw new UnauthorizedAccessException("El token no tiene un usuario válido.");
            return g;
        }
    }

    private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";

    private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    private string CorrelationId =>
        HttpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString("D");

    private string? BearerToken =>
        HttpContext.Request.Headers.Authorization.FirstOrDefault();

    [HttpPost("orders")]
    [Authorize(Policy = "ClienteAutenticado")]
    public async Task<IActionResult> CrearOrden(CancellationToken ct)
    {
        var raw = await ReadBodyRawAsync(ct);
        var request = JsonSerializer.Deserialize<CrearPayPalOrderApiRequest>(raw, JsonOpts)
            ?? throw new JsonException("JSON inválido.");

        CrearReservaOrquestadorDto? reservaDto = null;
        if (request.Reserva is not null)
        {
            reservaDto = new CrearReservaOrquestadorDto
            {
                AtGuid = request.Reserva.AtGuid,
                HorGuid = request.Reserva.HorGuid,
                FechaVisita = request.Reserva.FechaVisita,
                OrigenCanal = request.Reserva.OrigenCanal,
                Lineas = request.Reserva.Lineas
                    .Select(l => new LineaTicketOrquestadorDto { TckGuid = l.TckGuid, Cantidad = l.Cantidad })
                    .ToList(),
            };
        }

        var data = await _pagos.CrearOrdenAsync(
            reservaDto,
            request.RevGuid == Guid.Empty ? null : request.RevGuid,
            request.RevCodigo,
            UsuGuidActual,
            BearerToken,
            UsuarioAccion,
            IpActual,
            CorrelationId,
            ct);

        return Ok(new ApiItemResponse<object>(new
        {
            paypal_order_id = data.PaypalOrderId,
            moneda = data.Moneda,
            monto = data.Monto,
            rev_guid = data.RevGuid,
        }, 200, "Orden PayPal creada"));
    }

    [HttpPost("orders/capture")]
    [Authorize(Policy = "ClienteAutenticado")]
    [Obsolete("Use POST /api/v1/reservas/{rev_guid}/pagos/confirmacion con paypal_order_id")]
    public async Task<IActionResult> Capturar(CancellationToken ct)
    {
        var raw = await ReadBodyRawAsync(ct);
        var idemKey = RequireIdempotencyKey();
        if (idemKey is null)
            return BadRequestIdempotency();

        var route = HttpContext.Request.Path.ToString();
        var hash = Sha256Hex(raw);
        var cached = await _idem.ObtenerRespuestaSiExisteAsync(idemKey, route, hash, ct);
        if (cached is not null)
            return ReplayIdempotent(cached);

        var request = JsonSerializer.Deserialize<CapturarPayPalOrderApiRequest>(raw, JsonOpts)
            ?? throw new JsonException("JSON inválido.");

        if (string.IsNullOrWhiteSpace(request.PaypalOrderId))
            throw new JsonException("paypal_order_id es obligatorio.");

        var dto = new ConfirmarPagoOrquestadorDto
        {
            NombreReceptor = request.NombreReceptor,
            ApellidoReceptor = request.ApellidoReceptor,
            CorreoReceptor = request.CorreoReceptor,
            TelefonoReceptor = request.TelefonoReceptor,
            Observacion = request.Observacion,
        };

        try
        {
            var factura = await _pagos.CapturarYCompletarReservaAsync(
                request.RevGuid,
                request.RevCodigo,
                UsuGuidActual,
                request.PaypalOrderId.Trim(),
                dto,
                UsuarioAccion,
                IpActual,
                CorrelationId,
                ct);

            var envelope = new ApiItemResponse<object>(factura, 201, "Pago capturado y factura emitida");
            await SaveIdempotentAsync(idemKey, route, hash, 201, envelope, ct);
            return StatusCode(201, envelope);
        }
        catch (InvalidOperationException ex)
        {
            throw new ServiceUnavailableOrchestadorException(ex.Message);
        }
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        Request.EnableBuffering();
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var raw = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        if (!Request.Headers.TryGetValue("PAYPAL-TRANSMISSION-ID", out var tidVals)
            || !Request.Headers.TryGetValue("PAYPAL-TRANSMISSION-TIME", out var ttimeVals)
            || !Request.Headers.TryGetValue("PAYPAL-CERT-URL", out var certVals)
            || !Request.Headers.TryGetValue("PAYPAL-AUTH-ALGO", out var algoVals)
            || !Request.Headers.TryGetValue("PAYPAL-TRANSMISSION-SIG", out var sigVals))
        {
            return BadRequest(new ApiErrorResponse
            {
                Status = 400,
                Message = "Cabeceras PayPal incompletas",
                Details = new List<string> { "Se requieren PAYPAL-TRANSMISSION-ID, TIME, CERT-URL, AUTH-ALGO y TRANSMISSION-SIG." },
                Path = HttpContext.Request.Path.ToString(),
            });
        }

        var tid = tidVals.FirstOrDefault()?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(tid))
            return BadRequest();

        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(raw) ? "{}" : raw);
        var root = doc.RootElement;

        var ok = await _paypal.VerifyWebhookSignatureAsync(
            tid,
            ttimeVals.First() ?? "",
            certVals.First() ?? "",
            algoVals.First() ?? "",
            sigVals.First() ?? "",
            root,
            ct);

        if (!ok)
            return Unauthorized();

        var route = HttpContext.Request.Path.ToString();
        var hash = Sha256Hex(tid);
        var syntheticKey = "paypal-webhook:" + tid;
        var cached = await _idem.ObtenerRespuestaSiExisteAsync(syntheticKey, route, hash, ct);
        if (cached is not null)
            return ReplayIdempotent(cached);

        await _pagos.ProcesarWebhookAsync(raw, ct);

        var envelope = new ApiItemResponse<object>(new { ok = true }, 200, "Webhook procesado");
        await SaveIdempotentAsync(syntheticKey, route, hash, 200, envelope, ct);
        return Ok(envelope);
    }

    private string? RequireIdempotencyKey() =>
        HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var v)
            ? v.FirstOrDefault()?.Trim()
            : null;

    private IActionResult BadRequestIdempotency()
    {
        var body = new ApiErrorResponse
        {
            Status = 400,
            Message = "Falta Idempotency-Key",
            Details = new List<string> { "La captura PayPal requiere el header Idempotency-Key." },
            Path = HttpContext.Request.Path.ToString(),
        };
        return BadRequest(body);
    }

    private async Task<string> ReadBodyRawAsync(CancellationToken ct)
    {
        Request.EnableBuffering();
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var raw = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;
        return raw;
    }

    private static string Sha256Hex(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }

    private async Task SaveIdempotentAsync(string idemKey, string route, string hash, int httpStatus, object body, CancellationToken ct)
    {
        var wrapper = new Dictionary<string, object?> { ["http_status"] = httpStatus, ["body"] = body };
        var json = JsonSerializer.Serialize(wrapper, JsonOpts);
        await _idem.GuardarRespuestaAsync(idemKey, route, hash, json, ct);
    }

    private IActionResult ReplayIdempotent(string cachedJson)
    {
        using var doc = JsonDocument.Parse(cachedJson);
        var root = doc.RootElement;
        var http = root.GetProperty("http_status").GetInt32();
        var body = root.GetProperty("body").GetRawText();
        return new ContentResult { StatusCode = http, Content = body, ContentType = "application/json" };
    }
}
