using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Destinos;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Internal
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/destinos")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class DestinosController : ControllerBase
    {
        private readonly IDestinoAdminService _service;

        public DestinosController(IDestinoAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<DestinoResponse>>), 200)]
        public async Task<IActionResult> Listar()
        {
            var destinos = await _service.ListarAsync();
            return Ok(new ApiItemResponse<IReadOnlyList<DestinoResponse>>(destinos));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiItemResponse<DestinoResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        public async Task<IActionResult> Crear([FromBody] CrearDestinoRequest request)
        {
            var destino = await _service.CrearAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<DestinoResponse>(destino, 201));
        }

        [HttpPut("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<DestinoResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarDestinoRequest request)
        {
            var destino = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<DestinoResponse>(destino));
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
