using Atracciones.MsAtracciones.Api.Models.Common;
using Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;
using Atracciones.MsAtracciones.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsAtracciones.Api.Controllers.Public;

[ApiController]
[Route("api/v1/tickets")]
[Produces("application/json")]
public sealed class TicketsPublicController : ControllerBase
{
    private readonly IInventarioPublicAppService _service;

    public TicketsPublicController(IInventarioPublicAppService service) => _service = service;

    [HttpGet("{guid:guid}/horarios")]
    [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<HorarioProximoResponse>>), 200)]
    public async Task<IActionResult> ListarHorarios(Guid guid)
    {
        var horarios = await _service.ListarHorariosPorTicketAsync(guid);
        return Ok(new ApiItemResponse<IReadOnlyList<HorarioProximoResponse>>(horarios));
    }
}
