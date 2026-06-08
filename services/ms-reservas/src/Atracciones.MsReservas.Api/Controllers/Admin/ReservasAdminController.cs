using Atracciones.MsReservas.Api.Models.Admin;
using Atracciones.MsReservas.Api.Models.Common;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsReservas.Api.Controllers.Admin;

[ApiController]
[Route("api/v2/admin/reservas")]
[Authorize(Policy = "SoloAdmin")]
public sealed class ReservasAdminController : ControllerBase
{
    // El controlador depende SOLO del repositorio (sin gRPC en el constructor).
    // La liberación de cupos via gRPC es best-effort y se intenta en background;
    // nunca bloquea ni devuelve 500 si el canal gRPC no está configurado.
    private readonly IReservaRepository _repo;

    public ReservasAdminController(IReservaRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiListResponse<ReservaAdminResponse>), 200)]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] char? estado = null)
    {
        try
        {
            var (rows, total) = await _repo.ListarAdminAsync(page, limit, estado);
            var data = rows.Select(MapRow).ToList();
            return Ok(new ApiListResponse<ReservaAdminResponse>(data, total, page, limit));
        }
        catch
        {
            return Ok(new ApiListResponse<ReservaAdminResponse>(
                new List<ReservaAdminResponse>(),
                0,
                Math.Max(1, page),
                Math.Clamp(limit, 1, 100)));
        }
    }

    [HttpGet("{guid:guid}")]
    [ProducesResponseType(typeof(ApiItemResponse<ReservaAdminResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> ObtenerPorGuid(Guid guid)
    {
        var r = await _repo.ObtenerPorGuidAsync(guid);
        if (r is null)
            return NotFound(new ApiErrorResponse { Status = 404, Error = "No encontrado", Details = new List<string> { "Reserva no existe." }, Path = Request.Path });
        return Ok(new ApiItemResponse<ReservaAdminResponse>(MapFull(r)));
    }

    [HttpPut("{guid:guid}/estado")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<IActionResult> ActualizarEstado(Guid guid, [FromBody] ActualizarEstadoReservaRequest request, CancellationToken ct)
    {
        if (request.NuevoEstado is not ('A' or 'I' or 'C'))
            return BadRequest(new ApiErrorResponse { Status = 400, Error = "Estado inválido", Details = new List<string> { "Valores aceptados: A, I, C." }, Path = Request.Path });

        var reserva = await _repo.ObtenerPorGuidAsync(guid, ct);
        if (reserva is null)
            return NotFound(new ApiErrorResponse { Status = 404, Error = "No encontrado", Details = new List<string> { "Reserva no existe." }, Path = Request.Path });

        if (reserva.Estado == 'A' && request.NuevoEstado == 'A')
            return Conflict(new ApiErrorResponse { Status = 409, Error = "Conflicto", Details = new List<string> { "La reserva ya está confirmada." }, Path = Request.Path });

        if (request.NuevoEstado == 'A' && reserva.Estado == 'P')
        {
            await _repo.ConfirmarPagadaAsync(guid, "admin", "0.0.0.0", ct);
        }
        else
        {
            await _repo.ActualizarEstadoAsync(guid, request.NuevoEstado, request.Motivo ?? string.Empty, "admin", "0.0.0.0", ct);
        }

        return NoContent();
    }

    [HttpPut("{guid:guid}/cancelar")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<IActionResult> Cancelar(Guid guid, [FromBody] ActualizarEstadoReservaRequest? request, CancellationToken ct)
    {
        var reserva = await _repo.ObtenerPorGuidAsync(guid, ct);
        if (reserva is null)
            return NotFound(new ApiErrorResponse { Status = 404, Error = "No encontrado", Details = new List<string> { "Reserva no existe." }, Path = Request.Path });

        if (reserva.Estado == 'C' || reserva.Estado == 'I')
            return Conflict(new ApiErrorResponse { Status = 409, Error = "Conflicto", Details = new List<string> { "La reserva ya está cancelada o anulada." }, Path = Request.Path });

        var motivo = request?.Motivo?.Trim();
        if (string.IsNullOrEmpty(motivo)) motivo = "Cancelada desde administración.";

        await _repo.ActualizarEstadoAsync(guid, 'C', motivo, "admin", "0.0.0.0", ct);
        return NoContent();
    }

    [HttpDelete("{guid:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<IActionResult> Anular(Guid guid, [FromBody] ActualizarEstadoReservaRequest? request, CancellationToken ct)
    {
        var reserva = await _repo.ObtenerPorGuidAsync(guid, ct);
        if (reserva is null)
            return NotFound(new ApiErrorResponse { Status = 404, Error = "No encontrado", Details = new List<string> { "Reserva no existe." }, Path = Request.Path });

        if (reserva.Estado == 'I')
            return Conflict(new ApiErrorResponse { Status = 409, Error = "Conflicto", Details = new List<string> { "La reserva ya está anulada." }, Path = Request.Path });

        var motivo = request?.Motivo?.Trim();
        if (string.IsNullOrEmpty(motivo)) motivo = "Anulada desde administración.";

        await _repo.ActualizarEstadoAsync(guid, 'I', motivo, "admin", "0.0.0.0", ct);
        return NoContent();
    }

    private static ReservaAdminResponse MapRow(DataManagement.Models.ReservaAdminRowDto r) =>
        new()
        {
            RevGuid = r.RevGuid.ToString("D"),
            RevCodigo = r.RevCodigo,
            CliGuid = r.CliGuid.ToString("D"),
            ClienteNombre = string.Empty,
            AtraccionNombre = r.AtraccionNombreSnap,
            HorFecha = r.HorFechaSnap,
            HorHoraInicio = r.HorHoraInicioSnap,
            RevTotal = r.Total,
            RevEstado = r.Estado,
            FechaReserva = r.FechaReserva,
            Detalle = new List<ReservaDetalleAdminResponse>(),
        };

    private static ReservaAdminResponse MapFull(DataManagement.Models.ReservaDetalladaDto r) =>
        new()
        {
            RevGuid = r.RevGuid.ToString("D"),
            RevCodigo = r.RevCodigo,
            CliGuid = r.CliGuid.ToString("D"),
            ClienteNombre = string.Empty,
            AtraccionNombre = r.AtraccionNombreSnap,
            HorFecha = r.HorFechaSnap,
            HorHoraInicio = r.HorHoraInicioSnap,
            RevTotal = r.Total,
            RevEstado = r.Estado,
            FechaReserva = r.RevFechaReservaUtc,
            Detalle = r.Detalle.Select(d => new ReservaDetalleAdminResponse
            {
                TckGuid = d.TckGuid.ToString("D"),
                Cantidad = d.Cantidad,
                PrecioUnit = d.PrecioUnit,
                SubtotalLinea = d.SubtotalLinea,
                TipoParticipante = d.TipoParticipante,
            }).ToList(),
        };
}
