using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Atracciones;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Internal
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/atracciones")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class AtraccionesAdminController : ControllerBase
    {
        private readonly IAtraccionAdminService _service;

        public AtraccionesAdminController(IAtraccionAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiListResponse<AtraccionAdminResponse>), 200)]
        public async Task<IActionResult> Listar([FromQuery] AtraccionAdminFiltroRequest filtro)
        {
            var resultado = await _service.ListarAsync(filtro);
            var response = new ApiListResponse<AtraccionAdminResponse>(
                resultado.Items, resultado.TotalFiltrado, filtro.Page, filtro.Limit);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiItemResponse<AtraccionAdminResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Crear([FromBody] CrearAtraccionRequest request)
        {
            var atraccion = await _service.CrearAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<AtraccionAdminResponse>(atraccion, 201));
        }

        [HttpPut("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<AtraccionAdminResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarAtraccionRequest request)
        {
            var atraccion = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<AtraccionAdminResponse>(atraccion));
        }

        [HttpPatch("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<AtraccionAdminResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ActualizarParcial(Guid guid, [FromBody] ActualizarAtraccionRequest request)
        {
            var atraccion = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<AtraccionAdminResponse>(atraccion));
        }

        [HttpDelete("{guid:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Eliminar(Guid guid)
        {
            await _service.EliminarAsync(guid, UsuarioAccion, IpActual);
            return NoContent();
        }
    }
}
