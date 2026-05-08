using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Resenias;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Internal
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/resenias")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class ReseniasAdminController : ControllerBase
    {
        private readonly IReseniaAdminService _service;

        public ReseniasAdminController(IReseniaAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<ReseniaAdminResponse>>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Listar([FromQuery] Guid atraccionGuid)
        {
            var resenias = await _service.ListarPorAtraccionAsync(atraccionGuid);
            return Ok(new ApiItemResponse<IReadOnlyList<ReseniaAdminResponse>>(resenias));
        }

        [HttpGet("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<ReseniaAdminResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ObtenerPorGuid(Guid guid)
        {
            var resenia = await _service.ObtenerPorGuidAsync(guid);
            return Ok(new ApiItemResponse<ReseniaAdminResponse>(resenia));
        }

        [HttpPut("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<ReseniaAdminResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarReseniaRequest request)
        {
            var resenia = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<ReseniaAdminResponse>(resenia));
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
