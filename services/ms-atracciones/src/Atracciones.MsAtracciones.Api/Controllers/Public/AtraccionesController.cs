using Atracciones.MsAtracciones.Api.Mappers;
using Atracciones.MsAtracciones.Api.Models.Common;
using Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;
using Atracciones.MsAtracciones.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsAtracciones.Api.Controllers.Public;

[ApiController]
[Route("api/v1/atracciones")]
[Produces("application/json")]
public sealed class AtraccionesController : ControllerBase
{
    private readonly IInventarioPublicAppService _service;

    public AtraccionesController(IInventarioPublicAppService service) => _service = service;

    private string BaseUrl => $"{Request.Scheme}://{Request.Host}";
    private string QueryString => Request.QueryString.ToString();

    [HttpGet]
    [ProducesResponseType(typeof(ApiListResponse<AtraccionListadoResponse>), 200)]
    public async Task<IActionResult> Listar([FromQuery] AtraccionFiltroRequest filtro)
    {
        var resultado = await _service.ListarAsync(filtro, BaseUrl);
        var response = AtraccionesApiMapper.ToListadoResponse(resultado, BaseUrl, QueryString);
        return Ok(response);
    }

    [HttpGet("filtros")]
    [ProducesResponseType(typeof(ApiItemResponse<FiltrosAtraccionResponse>), 200)]
    public async Task<IActionResult> ObtenerFiltros()
    {
        var filtros = await _service.ObtenerFiltrosAsync();
        var response = AtraccionesApiMapper.ToFiltrosResponse(filtros);
        return Ok(response);
    }

    [HttpGet("{guid:guid}")]
    [ProducesResponseType(typeof(ApiItemResponse<AtraccionDetalleResponse>), 200)]
    public async Task<IActionResult> ObtenerPorGuid(Guid guid)
    {
        var detalle = await _service.ObtenerPorGuidAsync(guid, BaseUrl);
        var response = AtraccionesApiMapper.ToDetalleResponse(detalle);
        return Ok(response);
    }

    [HttpGet("{guid:guid}/tickets")]
    [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<TicketDisponibleResponse>>), 200)]
    public async Task<IActionResult> ListarTickets(Guid guid)
    {
        var tickets = await _service.ListarTicketsAsync(guid);
        return Ok(new ApiItemResponse<IReadOnlyList<TicketDisponibleResponse>>(tickets));
    }

    [HttpGet("{guid:guid}/horarios-disponibles")]
    [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<HorarioProximoResponse>>), 200)]
    public async Task<IActionResult> ListarHorariosDisponibles(Guid guid)
    {
        var horarios = await _service.ListarHorariosDisponiblesAsync(guid);
        return Ok(new ApiItemResponse<IReadOnlyList<HorarioProximoResponse>>(horarios));
    }
}
