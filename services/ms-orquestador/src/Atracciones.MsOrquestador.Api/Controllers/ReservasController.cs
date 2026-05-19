using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atracciones.MsOrquestador.Api.Models.Common;
using Atracciones.MsOrquestador.Api.Models.Reservas;
using Atracciones.MsOrquestador.Business.Models;
using Atracciones.MsOrquestador.Business.Services;
using Atracciones.MsOrquestador.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsOrquestador.Api.Controllers;

[ApiController]
[Route("api/v1/reservas")]
public sealed class ReservasController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private readonly IReservaOrquestacionService _orq;
    private readonly IPayPalPagosService _pagos;
    private readonly IIdempotencyRepository _idem;

    public ReservasController(
        IReservaOrquestacionService orq,
        IPayPalPagosService pagos,
        IIdempotencyRepository idem)
    {
        _orq = orq;
        _pagos = pagos;
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

    [HttpPost]
    [Authorize(Policy = "ClienteAutenticado")]
    public async Task<IActionResult> Crear(CancellationToken ct)
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

        var request = JsonSerializer.Deserialize<CrearReservaApiRequest>(raw, JsonOpts)
            ?? throw new JsonException("JSON inválido.");

        var dto = new CrearReservaOrquestadorDto
        {
            AtGuid = request.AtGuid,
            HorGuid = request.HorGuid,
            FechaVisita = request.FechaVisita,
            OrigenCanal = request.OrigenCanal,
            Lineas = request.Lineas.Select(l => new LineaTicketOrquestadorDto { TckGuid = l.TckGuid, Cantidad = l.Cantidad }).ToList(),
        };

        var data = await _orq.CrearReservaAsync(dto, UsuGuidActual, BearerToken, UsuarioAccion, IpActual, CorrelationId, ct);
        var envelope = new ApiItemResponse<object>(data, 201, "Reserva pendiente creada. Confirme el pago para finalizar.");
        await SaveIdempotentAsync(idemKey, route, hash, 201, envelope, ct);
        return StatusCode(201, envelope);
    }

    [HttpPost("{guid:guid}/pagos/confirmacion")]
    [Authorize(Policy = "ClienteAutenticado")]
    public async Task<IActionResult> ConfirmarPagoReserva(Guid guid, CancellationToken ct)
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

        var request = JsonSerializer.Deserialize<ConfirmarPagoApiRequest>(raw, JsonOpts)
            ?? throw new JsonException("JSON inválido.");

        var dto = new ConfirmarPagoOrquestadorDto
        {
            NombreReceptor = request.NombreReceptor,
            ApellidoReceptor = request.ApellidoReceptor,
            CorreoReceptor = request.CorreoReceptor,
            TelefonoReceptor = request.TelefonoReceptor,
            Observacion = request.Observacion,
        };

        FacturaStubResponseDto factura;
        if (!string.IsNullOrWhiteSpace(request.PaypalOrderId))
        {
            factura = await _pagos.CapturarYCompletarReservaAsync(
                guid,
                null,
                UsuGuidActual,
                request.PaypalOrderId.Trim(),
                dto,
                UsuarioAccion,
                IpActual,
                CorrelationId,
                ct);
        }
        else
        {
            factura = await _orq.CompletarPagoReservaYFacturaAsync(
                guid,
                dto,
                UsuarioAccion,
                IpActual,
                CorrelationId,
                compensarSiFallaFactura: true,
                ct);
        }

        var envelope = new ApiItemResponse<object>(factura, 201, "Pago confirmado y factura emitida");
        await SaveIdempotentAsync(idemKey, route, hash, 201, envelope, ct);
        return StatusCode(201, envelope);
    }

    [HttpPost("{guid:guid}/confirmar-pago")]
    [Authorize(Policy = "ClienteAutenticado")]
    [Obsolete("Use POST /api/v1/reservas/{guid}/pagos/confirmacion")]
    public Task<IActionResult> ConfirmarPagoLegacy(Guid guid, CancellationToken ct) =>
        ConfirmarPagoReserva(guid, ct);

    [HttpGet]
    [Authorize(Policy = "ClienteAutenticado")]
    public async Task<IActionResult> ListarMisReservas([FromQuery] int page = 1, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var (items, total) = await _orq.ListarMisReservasAsync(UsuGuidActual, page, limit, ct);
        return Ok(new ApiListResponse<object>(items, total, page, limit));
    }

    [HttpGet("{guid:guid}")]
    [Authorize(Policy = "ClienteAutenticado")]
    public async Task<IActionResult> ObtenerPorGuid(Guid guid, CancellationToken ct)
    {
        var data = await _orq.ObtenerReservaAsync(guid, UsuGuidActual, ct);
        return Ok(new ApiItemResponse<object>(data));
    }

    [HttpPut("{guid:guid}/cancelar")]
    [Authorize(Policy = "ClienteAutenticado")]
    public async Task<IActionResult> Cancelar(Guid guid, [FromBody] CancelarReservaApiRequest? request, CancellationToken ct)
    {
        await _orq.CancelarReservaAsync(guid, request?.Motivo ?? string.Empty, UsuGuidActual, UsuarioAccion, IpActual, CorrelationId, ct);
        return NoContent();
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
            Details = new List<string> { "Las operaciones POST del orquestador requieren el header Idempotency-Key." },
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
