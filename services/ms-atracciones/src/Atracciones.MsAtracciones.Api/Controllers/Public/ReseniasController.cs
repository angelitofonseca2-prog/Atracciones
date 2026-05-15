using System.Security.Claims;
using Atracciones.MsAtracciones.Business.Services;
using Atracciones.MsAtracciones.Api.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsAtracciones.Api.Controllers.Public;

[ApiController]
[Route("api/v1/resenias")]
[Produces("application/json")]
public sealed class ReseniasController : ControllerBase
{
    private readonly IReseniaAppService _service;

    public ReseniasController(IReseniaAppService service) => _service = service;

    /// <summary>Lista reseñas activas de una atracción con paginación.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid at_guid,
        [FromQuery] int page = 1,
        [FromQuery] int page_size = 10,
        CancellationToken cancellationToken = default)
    {
        if (at_guid == Guid.Empty)
            return BadRequest(new { status = 400, error = "Se requiere el parámetro at_guid." });

        var result = await _service.ListarPorAtraccionAsync(at_guid, page, page_size, cancellationToken);
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
            }
        });
    }

    /// <summary>Crea una reseña. Requiere autenticación; un rev_guid solo puede tener una reseña.</summary>
    [HttpPost]
    [Authorize(Policy = "ClienteAutenticado")]
    public async Task<IActionResult> Crear(
        [FromBody] CrearReseniaRequest request,
        CancellationToken cancellationToken)
    {
        var usuGuid = User.FindFirstValue("usu_guid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonimo";
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

        var result = await _service.CrearAsync(request, usuGuid, ip, cancellationToken);
        return StatusCode(201, new ApiItemResponse<ReseniaItemResponse>(result, 201));
    }
}
