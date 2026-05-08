using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/tickets")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketAdminService _service;

        public TicketsController(ITicketAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<TicketResponse>>), 200)]
        public async Task<IActionResult> Listar()
        {
            var tickets = await _service.ListarTicketsAsync();
            return Ok(new ApiItemResponse<IReadOnlyList<TicketResponse>>(tickets));
        }

        [HttpGet("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<TicketResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ObtenerPorGuid(Guid guid)
        {
            var ticket = await _service.ObtenerTicketPorGuidAsync(guid);
            return Ok(new ApiItemResponse<TicketResponse>(ticket));
        }

        [HttpGet("~/api/v1/admin/atracciones/{atraccionGuid:guid}/tickets")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<TicketResponse>>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ListarPorAtraccion(Guid atraccionGuid)
        {
            var tickets = await _service.ListarTicketsPorAtraccionAsync(atraccionGuid);
            return Ok(new ApiItemResponse<IReadOnlyList<TicketResponse>>(tickets));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiItemResponse<TicketResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Crear([FromBody] CrearTicketRequest request)
        {
            var ticket = await _service.CrearTicketAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<TicketResponse>(ticket, 201));
        }

        [HttpPut("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<TicketResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarTicketRequest request)
        {
            var ticket = await _service.ActualizarTicketAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<TicketResponse>(ticket));
        }

        [HttpDelete("{guid:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Eliminar(Guid guid)
        {
            await _service.EliminarTicketAsync(guid, UsuarioAccion, IpActual);
            return NoContent();
        }

        [HttpPost("horarios")]
        [ProducesResponseType(typeof(ApiItemResponse<HorarioResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> CrearHorario([FromBody] CrearHorarioRequest request)
        {
            var horario = await _service.CrearHorarioAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<HorarioResponse>(horario, 201));
        }

        [HttpGet("~/api/v1/admin/horarios")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<HorarioResponse>>), 200)]
        public async Task<IActionResult> ListarHorarios()
        {
            var horarios = await _service.ListarHorariosAsync();
            return Ok(new ApiItemResponse<IReadOnlyList<HorarioResponse>>(horarios));
        }

        [HttpGet("~/api/v1/admin/horarios/{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<HorarioResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ObtenerHorarioPorGuid(Guid guid)
        {
            var horario = await _service.ObtenerHorarioPorGuidAsync(guid);
            return Ok(new ApiItemResponse<HorarioResponse>(horario));
        }

        [HttpPut("~/api/v1/admin/horarios/{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<HorarioResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> ActualizarHorario(Guid guid, [FromBody] ActualizarHorarioRequest request)
        {
            var horario = await _service.ActualizarHorarioAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<HorarioResponse>(horario));
        }

        [HttpDelete("~/api/v1/admin/horarios/{guid:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> EliminarHorario(Guid guid)
        {
            await _service.EliminarHorarioAsync(guid, UsuarioAccion, IpActual);
            return NoContent();
        }

        [HttpGet("{ticketGuid:guid}/horarios")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<HorarioResponse>>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ListarHorariosPorTicket(Guid ticketGuid)
        {
            var horarios = await _service.ListarHorariosPorTicketAsync(ticketGuid);
            return Ok(new ApiItemResponse<IReadOnlyList<HorarioResponse>>(horarios));
        }

        [HttpGet("~/api/v1/admin/atracciones/{atraccionGuid:guid}/horarios")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<HorarioResponse>>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ListarHorariosPorAtraccion(Guid atraccionGuid)
        {
            var horarios = await _service.ListarHorariosPorAtraccionAsync(atraccionGuid);
            return Ok(new ApiItemResponse<IReadOnlyList<HorarioResponse>>(horarios));
        }
    }
}
