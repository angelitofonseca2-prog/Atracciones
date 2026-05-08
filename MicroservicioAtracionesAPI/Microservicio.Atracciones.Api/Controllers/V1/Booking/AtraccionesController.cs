using Asp.Versioning;
using Microservicio.Atracciones.Api.Helpers;
using Microservicio.Atracciones.Api.Mappers.Public;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Public.Atracciones;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio.Atracciones.Api.Controllers.V1.Booking;

/// <summary>
/// Expone los 3 endpoints públicos de atracciones del contrato v2.0.
/// Sin autenticación — acceso abierto al frontend.
/// 
///   GET /api/v1/atracciones          → listado paginado con filtros
///   GET /api/v1/atracciones/filtros  → opciones de filtrado por ciudad
///   GET /api/v1/atracciones/{guid}   → detalle completo
/// 
/// IMPORTANTE: /filtros debe ir ANTES de /{guid} en el routing para
/// que ASP.NET no interprete "filtros" como un GUID.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v1/atracciones")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), 500)]
public class AtraccionesController : ControllerBase
{
    private readonly IAtraccionPublicService _service;

    public AtraccionesController(IAtraccionPublicService service)
        => _service = service;

    private string BaseUrl => $"{Request.Scheme}://{Request.Host}";
    private string QueryString => Request.QueryString.ToString();

    // ----------------------------------------------------------------
    //  GET /api/v1/atracciones
    //  Listado paginado con filtros opcionales
    //  Contrato sección 2.1 + 3.1 + 4.1
    // ----------------------------------------------------------------
    [HttpGet]
    [EndpointName(EndpointNames.ListarAtracciones)]
    [ResponseCache(CacheProfileName = CacheProfileNames.Listado, VaryByQueryKeys = new[] { "ciudad", "tipo", "subtipo", "etiqueta", "idioma", "calificacion_min", "horario", "disponible", "ordenar_por", "page", "limit" })]
    [ProducesResponseType(typeof(ApiListResponse<AtraccionListadoResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> Listar([FromQuery] AtraccionFiltroRequest filtro)
    {
        var resultado = await _service.ListarAsync(filtro, BaseUrl);
        var response = AtraccionesApiMapper.ToListadoResponse(
            resultado, BaseUrl, QueryString, filtro.OrdenarPor);

        return Ok(response);
    }

    // ----------------------------------------------------------------
    //  GET /api/v1/atracciones/filtros
    //  Opciones de filtrado por ciudad — debe ir ANTES de /{guid}
    //  Contrato sección 2.3 + 4.3
    // ----------------------------------------------------------------
    [HttpGet("filtros")]
    [EndpointName(EndpointNames.ObtenerFiltros)]
    //[ResponseCache(CacheProfileName = CacheProfileNames.Filtros, VaryByQueryKeys = new[] { "ciudad" })]
    [ProducesResponseType(typeof(ApiItemResponse<FiltrosAtraccionResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> ObtenerFiltros()
    {
        var filtros = await _service.ObtenerFiltrosAsync();
        var response = AtraccionesApiMapper.ToFiltrosResponse(filtros);
        return Ok(response);
    }

    // ----------------------------------------------------------------
    //  GET /api/v1/atracciones/{guid}
    //  Detalle completo por GUID — nunca por at_id interno
    //  Contrato sección 2.2 + 3.3 + 4.2
    // ----------------------------------------------------------------
    [HttpGet("{guid:guid}")]
    [EndpointName(EndpointNames.ObtenerAtraccionPorGuid)]
    [ResponseCache(CacheProfileName = CacheProfileNames.Detalle)]
    [ProducesResponseType(typeof(ApiItemResponse<AtraccionDetalleResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> ObtenerPorGuid(Guid guid)
    {
        var detalle = await _service.ObtenerPorGuidAsync(guid, BaseUrl);
        var response = AtraccionesApiMapper.ToDetalleResponse(detalle);
        return Ok(response);
    }

    [HttpGet("{guid:guid}/tickets")]
    [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<TicketDisponibleResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> ListarTickets(Guid guid)
    {
        var tickets = await _service.ListarTicketsAsync(guid);
        return Ok(new ApiItemResponse<IReadOnlyList<TicketDisponibleResponse>>(tickets));
    }

    [HttpGet("{guid:guid}/horarios-disponibles")]
    [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<HorarioProximoResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> ListarHorariosDisponibles(Guid guid)
    {
        var horarios = await _service.ListarHorariosDisponiblesAsync(guid);
        return Ok(new ApiItemResponse<IReadOnlyList<HorarioProximoResponse>>(horarios));
    }
}
