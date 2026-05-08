using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Imagenes;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Internal
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/imagenes")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class ImagenesController : ControllerBase
    {
        private readonly IImagenAdminService _service;

        public ImagenesController(IImagenAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<ImagenResponse>>), 200)]
        public async Task<IActionResult> Listar()
        {
            var imagenes = await _service.ListarAsync();
            return Ok(new ApiItemResponse<IReadOnlyList<ImagenResponse>>(imagenes));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiItemResponse<ImagenResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Crear([FromBody] CrearImagenRequest request)
        {
            var imagen = await _service.CrearAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<ImagenResponse>(imagen, 201));
        }

        [HttpPut("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<ImagenResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarImagenRequest request)
        {
            var imagen = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<ImagenResponse>(imagen));
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
