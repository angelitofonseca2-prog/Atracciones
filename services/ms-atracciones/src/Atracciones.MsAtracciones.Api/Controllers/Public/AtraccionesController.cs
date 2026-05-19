using System.Security.Claims;
using Atracciones.MsAtracciones.Api.Mappers;
using Atracciones.MsAtracciones.Api.Models.Common;
using Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;
using Atracciones.MsAtracciones.Business.Services;
using ReseniaItemResponse = Atracciones.MsAtracciones.Business.Services.ReseniaItemResponse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsAtracciones.Api.Controllers.Public;

[ApiController]
[Route("api/v2/atracciones")]
[Produces("application/json")]
public sealed class AtraccionesController : ControllerBase
{
    private readonly IInventarioPublicAppService _service;
    private readonly IReseniaAppService _resenias;

    public AtraccionesController(IInventarioPublicAppService service, IReseniaAppService resenias)
    {
        _service = service;
        _resenias = resenias;
    }

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
        var payload = BookingPublicResponseMapper.ToDetalleBooking(detalle);
        return Ok(new ApiItemResponse<object>(payload, 200, "Operación exitosa"));
    }

    [HttpGet("{guid:guid}/tickets")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ListarTickets(Guid guid)
    {
        var tickets = await _service.ListarTicketsAsync(guid);
        var items = tickets.Select(BookingPublicResponseMapper.ToTicketSimple).ToList();
        return Ok(new { status = 200, data = items });
    }

    [HttpGet("{guid:guid}/horarios/{horarioId:guid}/tickets")]
    [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<TicketHorarioDisponibleResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> ListarTicketsPorHorario(Guid guid, Guid horarioId)
    {
        var tickets = await _service.ListarTicketsPorHorarioAsync(guid, horarioId);
        var items = tickets.Select(t => new
        {
            tck_guid = t.TckGuid,
            tipo = t.Tipo,
            precio = t.Precio,
            moneda = t.Moneda,
        }).ToList();
        return Ok(new ApiItemResponse<object>(new { items }, 200, "Consulta exitosa"));
    }

    [HttpGet("{guid:guid}/horarios")]
    [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<HorarioProximoResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> ListarHorarios(Guid guid, [FromQuery] bool disponibles = true)
    {
        var horarios = await _service.ListarHorariosAsync(guid, disponibles);
        var items = horarios.Select(BookingPublicResponseMapper.ToHorarioSimple).ToList();
        return Ok(new { status = 200, data = items });
    }

    [HttpGet("{guid:guid}/resenias")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ListarResenias(
        Guid guid,
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _resenias.ListarPorAtraccionAsync(guid, page, page_size, cancellationToken);
        return Ok(new
        {
            status = 200,
            message = "Consulta exitosa",
            data = result.Items,
            pagination = new
            {
                page = result.Page,
                page_size = result.PageSize,
                total = result.Total,
                total_pages = result.TotalPages,
            },
        });
    }

    [HttpPost("{guid:guid}/resenias")]
    [Authorize(Policy = "ClienteAutenticado")]
    [ProducesResponseType(typeof(ApiItemResponse<ReseniaItemResponse>), 201)]
    public async Task<IActionResult> CrearResenia(
        Guid guid,
        [FromBody] CrearReseniaBodyRequest request,
        CancellationToken cancellationToken)
    {
        var usuGuid = User.FindFirstValue("usu_guid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonimo";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        var crear = new CrearReseniaRequest
        {
            AtGuid = guid,
            RevGuid = request.RevGuid,
            Comentario = request.Comentario,
            Rating = request.Rating,
        };

        var result = await _resenias.CrearAsync(crear, usuGuid, ip, cancellationToken);
        return StatusCode(201, new ApiItemResponse<ReseniaItemResponse>(result, 201));
    }
}
