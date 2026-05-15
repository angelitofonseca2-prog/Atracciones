using Atracciones.MsReservas.Api.Models.Admin;
using Atracciones.MsReservas.Api.Models.Common;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsReservas.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/reservas")]
[Authorize(Policy = "SoloAdmin")]
public sealed class ReservasAdminController : ControllerBase
{
    private readonly IReservaRepository _repo;

    public ReservasAdminController(IReservaRepository repo) => _repo = repo;

    [HttpGet]
    [ProducesResponseType(typeof(ApiListResponse<ReservaAdminResponse>), 200)]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] char? estado = null)
    {
        var (rows, total) = await _repo.ListarAdminAsync(page, limit, estado);
        var data = rows.Select(MapRow).ToList();
        return Ok(new ApiListResponse<ReservaAdminResponse>(data, total, page, limit));
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
