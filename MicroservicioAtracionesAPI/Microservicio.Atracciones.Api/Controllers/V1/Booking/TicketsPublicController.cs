using Asp.Versioning;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Public.Atracciones;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v1/tickets")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public class TicketsPublicController : ControllerBase
    {
        private readonly IAtraccionPublicService _service;

        public TicketsPublicController(IAtraccionPublicService service)
            => _service = service;

        [HttpGet("{guid:guid}/horarios")]
        [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<HorarioProximoResponse>>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        public async Task<IActionResult> ListarHorarios(Guid guid)
        {
            var horarios = await _service.ListarHorariosPorTicketAsync(guid);
            return Ok(new ApiItemResponse<IReadOnlyList<HorarioProximoResponse>>(horarios));
        }
    }
}
