using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Reservas;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/reservas")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class ReservasAdminController : ControllerBase
    {
        private readonly IReservaAdminService _service;

        public ReservasAdminController(IReservaAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiListResponse<ReservaAdminResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        public async Task<IActionResult> Listar([FromQuery] ReservaAdminFiltroRequest filtro)
        {
            var resultado = await _service.ListarAsync(filtro);
            var response = new ApiListResponse<ReservaAdminResponse>(resultado.Items, resultado.TotalFiltrado, filtro.Page, filtro.Limit);
            return Ok(response);
        }

        [HttpGet("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<ReservaAdminResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ObtenerPorGuid(Guid guid)
        {
            var reserva = await _service.ObtenerPorGuidAsync(guid);
            return Ok(new ApiItemResponse<ReservaAdminResponse>(reserva));
        }

        [HttpPut("{guid:guid}/estado")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> ActualizarEstado(Guid guid, [FromBody] ActualizarEstadoReservaRequest request)
        {
            await _service.ActualizarEstadoAsync(guid, request, UsuarioAccion, IpActual);
            return NoContent();
        }

        [HttpPut("{guid:guid}/cancelar")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Cancelar(Guid guid, [FromBody] ActualizarEstadoReservaRequest request)
        {
            request.NuevoEstado = 'C';
            await _service.ActualizarEstadoAsync(guid, request, UsuarioAccion, IpActual);
            return NoContent();
        }

        [HttpDelete("{guid:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Anular(Guid guid, [FromBody] ActualizarEstadoReservaRequest? request)
        {
            await _service.ActualizarEstadoAsync(
                guid,
                new ActualizarEstadoReservaRequest
                {
                    NuevoEstado = 'I',
                    Motivo = request?.Motivo ?? "Anulada desde administración."
                },
                UsuarioAccion,
                IpActual);
            return NoContent();
        }
    }
}
