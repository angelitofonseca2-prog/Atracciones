using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Clientes;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/clientes")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteAdminService _service;

        public ClientesController(IClienteAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiListResponse<ClienteResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        public async Task<IActionResult> Listar([FromQuery] ClienteFiltroRequest filtro)
        {
            var resultado = await _service.ListarAsync(filtro);
            var response = new ApiListResponse<ClienteResponse>(resultado.Items, resultado.TotalFiltrado, filtro.Page, filtro.Limit);
            return Ok(response);
        }

        [HttpGet("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<ClienteResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ObtenerPorGuid(Guid guid)
        {
            var cliente = await _service.ObtenerPorGuidAsync(guid);
            return Ok(new ApiItemResponse<ClienteResponse>(cliente));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiItemResponse<ClienteResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        public async Task<IActionResult> Crear([FromBody] CrearClienteRequest request)
        {
            var cliente = await _service.CrearAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<ClienteResponse>(cliente, 201));
        }

        [HttpPut("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<ClienteResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarClienteRequest request)
        {
            var cliente = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual);
            return Ok(new ApiItemResponse<ClienteResponse>(cliente));
        }
    }
}
