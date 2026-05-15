using Atracciones.MsAtracciones.Api.Models.Common;
using Atracciones.MsAtracciones.Business.Dtos.Admin.Tickets;
using Atracciones.MsAtracciones.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Atracciones.MsAtracciones.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/tickets")]
[Authorize(Policy = "SoloAdmin")]
[Produces("application/json")]
public sealed class TicketsAdminController : ControllerBase
{
    private readonly IInventarioTicketAdminAppService _service;

    public TicketsAdminController(IInventarioTicketAdminAppService service) => _service = service;

    private string UsuarioAccion => User.FindFirstValue("login") ?? User.FindFirstValue(ClaimTypes.Name) ?? "sistema";
    private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var tickets = await _service.ListarTicketsAsync();
        return Ok(new ApiItemResponse<IReadOnlyList<TicketResponse>>(tickets));
    }

    [HttpGet("{guid:guid}")]
    public async Task<IActionResult> ObtenerPorGuid(Guid guid)
    {
        var ticket = await _service.ObtenerTicketPorGuidAsync(guid);
        return Ok(new ApiItemResponse<TicketResponse>(ticket));
    }

    [HttpGet("~/api/v1/admin/atracciones/{atraccionGuid:guid}/tickets")]
    public async Task<IActionResult> ListarPorAtraccion(Guid atraccionGuid)
    {
        var tickets = await _service.ListarTicketsPorAtraccionAsync(atraccionGuid);
        return Ok(new ApiItemResponse<IReadOnlyList<TicketResponse>>(tickets));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTicketRequest request)
    {
        var ticket = await _service.CrearTicketAsync(request, UsuarioAccion, IpActual);
        return StatusCode(201, new ApiItemResponse<TicketResponse>(ticket, 201));
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarTicketRequest request)
    {
        var ticket = await _service.ActualizarTicketAsync(guid, request, UsuarioAccion, IpActual);
        return Ok(new ApiItemResponse<TicketResponse>(ticket));
    }

    [HttpDelete("{guid:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Eliminar(Guid guid)
    {
        await _service.EliminarTicketAsync(guid, UsuarioAccion, IpActual);
        return NoContent();
    }

    [HttpPost("horarios")]
    public async Task<IActionResult> CrearHorario([FromBody] CrearHorarioRequest request)
    {
        var horario = await _service.CrearHorarioAsync(request, UsuarioAccion, IpActual);
        return StatusCode(201, new ApiItemResponse<HorarioResponse>(horario, 201));
    }

    [HttpGet("~/api/v1/admin/horarios")]
    public async Task<IActionResult> ListarHorarios()
    {
        var horarios = await _service.ListarHorariosAsync();
        return Ok(new ApiItemResponse<IReadOnlyList<HorarioResponse>>(horarios));
    }

    [HttpGet("~/api/v1/admin/horarios/{guid:guid}")]
    public async Task<IActionResult> ObtenerHorarioPorGuid(Guid guid)
    {
        var horario = await _service.ObtenerHorarioPorGuidAsync(guid);
        return Ok(new ApiItemResponse<HorarioResponse>(horario));
    }

    [HttpPut("~/api/v1/admin/horarios/{guid:guid}")]
    public async Task<IActionResult> ActualizarHorario(Guid guid, [FromBody] ActualizarHorarioRequest request)
    {
        var horario = await _service.ActualizarHorarioAsync(guid, request, UsuarioAccion, IpActual);
        return Ok(new ApiItemResponse<HorarioResponse>(horario));
    }

    [HttpDelete("~/api/v1/admin/horarios/{guid:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> EliminarHorario(Guid guid)
    {
        await _service.EliminarHorarioAsync(guid, UsuarioAccion, IpActual);
        return NoContent();
    }

    [HttpGet("{ticketGuid:guid}/horarios")]
    public async Task<IActionResult> ListarHorariosPorTicket(Guid ticketGuid)
    {
        var horarios = await _service.ListarHorariosPorTicketAsync(ticketGuid);
        return Ok(new ApiItemResponse<IReadOnlyList<HorarioResponse>>(horarios));
    }

    [HttpGet("~/api/v1/admin/atracciones/{atraccionGuid:guid}/horarios")]
    public async Task<IActionResult> ListarHorariosPorAtraccion(Guid atraccionGuid)
    {
        var horarios = await _service.ListarHorariosPorAtraccionAsync(atraccionGuid);
        return Ok(new ApiItemResponse<IReadOnlyList<HorarioResponse>>(horarios));
    }
}
