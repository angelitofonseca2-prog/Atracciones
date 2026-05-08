using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Usuarios;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Internal
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/usuarios")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioAdminService _service;

        public UsuariosController(IUsuarioAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<UsuarioResponse>>), 200)]
        public async Task<IActionResult> Listar()
        {
            var usuarios = await _service.ListarAsync();
            return Ok(new ApiItemResponse<IReadOnlyList<UsuarioResponse>>(usuarios));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiItemResponse<UsuarioResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioRequest request)
        {
            var usuario = await _service.CrearAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<UsuarioResponse>(usuario, 201));
        }

        [HttpPut("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<UsuarioResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarUsuarioRequest request)
        {
            var usuario = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<UsuarioResponse>(usuario));
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
