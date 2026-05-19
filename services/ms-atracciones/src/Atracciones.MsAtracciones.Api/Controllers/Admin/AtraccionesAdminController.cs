using Atracciones.MsAtracciones.Api.Models.Common;
using Atracciones.MsAtracciones.Business.Dtos.Admin.Atracciones;
using Atracciones.MsAtracciones.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Atracciones.MsAtracciones.Api.Controllers.Admin;

[ApiController]
[Route("api/v2/admin/atracciones")]
[Authorize(Policy = "SoloAdmin")]
[Produces("application/json")]
public sealed class AtraccionesAdminController : ControllerBase
{
    private readonly IInventarioAdminAppService _service;

    public AtraccionesAdminController(IInventarioAdminAppService service) => _service = service;

    private string UsuarioAccion => User.FindFirstValue("login") ?? User.FindFirstValue(ClaimTypes.Name) ?? "sistema";
    private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    [HttpGet]
    [ProducesResponseType(typeof(ApiListResponse<AtraccionAdminResponse>), 200)]
    public async Task<IActionResult> Listar([FromQuery] AtraccionAdminFiltroRequest filtro)
    {
        var resultado = await _service.ListarAsync(filtro);
        return Ok(new ApiListResponse<AtraccionAdminResponse>
        {
            Status = 200,
            Message = "Consulta exitosa",
            Data = resultado.Items.ToList(),
            Pagination = new PaginationResponse
            {
                Page = resultado.Page,
                Limit = resultado.Limit,
                Total = resultado.TotalFiltrado,
                TotalPages = resultado.TotalPaginas,
            },
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiItemResponse<AtraccionAdminResponse>), 201)]
    public async Task<IActionResult> Crear([FromBody] CrearAtraccionRequest request)
    {
        var atraccion = await _service.CrearAsync(request, UsuarioAccion, IpActual);
        return StatusCode(201, new ApiItemResponse<AtraccionAdminResponse>(atraccion, 201));
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarAtraccionRequest request)
    {
        var atraccion = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual);
        return Ok(new ApiItemResponse<AtraccionAdminResponse>(atraccion));
    }

    [HttpPatch("{guid:guid}")]
    public async Task<IActionResult> ActualizarParcial(Guid guid, [FromBody] ActualizarAtraccionRequest request)
    {
        var atraccion = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual);
        return Ok(new ApiItemResponse<AtraccionAdminResponse>(atraccion));
    }

    [HttpDelete("{guid:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Eliminar(Guid guid)
    {
        await _service.EliminarAsync(guid, UsuarioAccion, IpActual);
        return NoContent();
    }
}
