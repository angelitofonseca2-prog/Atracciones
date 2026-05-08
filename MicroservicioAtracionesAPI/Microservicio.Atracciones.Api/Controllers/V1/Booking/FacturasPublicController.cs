using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/facturas")]
    [Authorize(Policy = "ClienteAutenticado")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class FacturasPublicController : ControllerBase
    {
        private readonly IFacturaPublicService _service;

        public FacturasPublicController(IFacturaPublicService service)
            => _service = service;

        private Guid UsuGuidActual
        {
            get
            {
                var claim = User.FindFirstValue("usu_guid");
                if (!Guid.TryParse(claim, out var usuGuid))
                    throw new UnauthorizedBusinessException("El token no tiene un usuario válido.");

                return usuGuid;
            }
        }

        [HttpGet("mis-facturas")]
        [ProducesResponseType(typeof(ApiListResponse<FacturaResponse>), 200)]
        public async Task<IActionResult> ListarMisFacturas([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var resultado = await _service.ListarMisFacturasAsync(UsuGuidActual, page, limit);
            return Ok(new ApiListResponse<FacturaResponse>(resultado.Items, resultado.TotalFiltrado, page, limit));
        }
    }
}
