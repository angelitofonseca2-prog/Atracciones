using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Catalogos;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Internal
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class CatalogosAdminController : ControllerBase
    {
        private readonly ICatalogoAdminService _service;

        public CatalogosAdminController(ICatalogoAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet("categorias")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<CategoriaResponse>>), 200)]
        public async Task<IActionResult> ListarCategorias()
        {
            var response = await _service.ListarCategoriasAsync();
            return Ok(new ApiItemResponse<IReadOnlyList<CategoriaResponse>>(response));
        }

        [HttpPost("categorias")]
        [ProducesResponseType(typeof(ApiItemResponse<CategoriaResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaRequest request)
        {
            var response = await _service.CrearCategoriaAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<CategoriaResponse>(response, 201));
        }

        [HttpPut("categorias/{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<CategoriaResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ActualizarCategoria(Guid guid, [FromBody] ActualizarCategoriaRequest request)
        {
            var response = await _service.ActualizarCategoriaAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<CategoriaResponse>(response));
        }

        [HttpDelete("categorias/{guid:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> EliminarCategoria(Guid guid)
        {
            await _service.EliminarCategoriaAsync(guid, UsuarioAccion, IpActual);
            return NoContent();
        }

        [HttpGet("idiomas")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<IdiomaResponse>>), 200)]
        public async Task<IActionResult> ListarIdiomas()
        {
            var response = await _service.ListarIdiomasAsync();
            return Ok(new ApiItemResponse<IReadOnlyList<IdiomaResponse>>(response));
        }

        [HttpPost("idiomas")]
        [ProducesResponseType(typeof(ApiItemResponse<IdiomaResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> CrearIdioma([FromBody] CrearIdiomaRequest request)
        {
            var response = await _service.CrearIdiomaAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<IdiomaResponse>(response, 201));
        }

        [HttpPut("idiomas/{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<IdiomaResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> ActualizarIdioma(Guid guid, [FromBody] ActualizarIdiomaRequest request)
        {
            var response = await _service.ActualizarIdiomaAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<IdiomaResponse>(response));
        }

        [HttpDelete("idiomas/{guid:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> EliminarIdioma(Guid guid)
        {
            await _service.EliminarIdiomaAsync(guid, UsuarioAccion, IpActual);
            return NoContent();
        }

        [HttpGet("incluye")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<IncluyeResponse>>), 200)]
        public async Task<IActionResult> ListarIncluye()
        {
            var response = await _service.ListarIncluyeAsync();
            return Ok(new ApiItemResponse<IReadOnlyList<IncluyeResponse>>(response));
        }

        [HttpPost("incluye")]
        [ProducesResponseType(typeof(ApiItemResponse<IncluyeResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        public async Task<IActionResult> CrearIncluye([FromBody] CrearIncluyeRequest request)
        {
            var response = await _service.CrearIncluyeAsync(request);
            return StatusCode(201, new ApiItemResponse<IncluyeResponse>(response, 201));
        }

        [HttpPut("incluye/{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<IncluyeResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ActualizarIncluye(Guid guid, [FromBody] ActualizarIncluyeRequest request)
        {
            var response = await _service.ActualizarIncluyeAsync(guid, request);
            return Ok(new ApiItemResponse<IncluyeResponse>(response));
        }

        [HttpDelete("incluye/{guid:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> EliminarIncluye(Guid guid)
        {
            await _service.EliminarIncluyeAsync(guid);
            return NoContent();
        }
    }
}
