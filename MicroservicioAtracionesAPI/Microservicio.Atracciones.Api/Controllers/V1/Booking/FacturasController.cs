using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.Business.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/admin/facturas")]
    [Authorize(Policy = "SoloAdmin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class FacturasController : ControllerBase
    {
        private readonly IFacturaAdminService _service;

        public FacturasController(IFacturaAdminService service)
            => _service = service;

        private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
        private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        [HttpGet]
        [ProducesResponseType(typeof(ApiListResponse<FacturaResponse>), 200)]
        public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var resultado = await _service.ListarAsync(page, limit);
            var response = new ApiListResponse<FacturaResponse>(resultado.Items, resultado.TotalFiltrado, page, limit);
            return Ok(response);
        }

        [HttpGet("{guid:guid}")]
        [ProducesResponseType(typeof(ApiItemResponse<FacturaResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ObtenerPorGuid(Guid guid)
        {
            var factura = await _service.ObtenerPorGuidAsync(guid);
            return Ok(new ApiItemResponse<FacturaResponse>(factura));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiItemResponse<FacturaResponse>), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        public async Task<IActionResult> Generar([FromBody] GenerarFacturaRequest request)
        {
            var factura = await _service.GenerarAsync(request, UsuarioAccion, IpActual);
            return StatusCode(201, new ApiItemResponse<FacturaResponse>(factura, 201));
        }
    }
}
