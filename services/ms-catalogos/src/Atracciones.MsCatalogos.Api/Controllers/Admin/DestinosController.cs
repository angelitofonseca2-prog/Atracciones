using System.Security.Claims;
using Atracciones.MsCatalogos.Api.Models;
using Atracciones.MsCatalogos.Business;
using Atracciones.MsCatalogos.Business.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsCatalogos.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/destinos")]
[Authorize(Policy = "SoloAdmin")]
[Produces("application/json")]
public sealed class DestinosController : ControllerBase
{
    private readonly ICatalogosAdminAppService _service;

    public DestinosController(ICatalogosAdminAppService service) => _service = service;

    private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
    private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var destinos = await _service.ListDestinosAsync(cancellationToken);
        return Ok(new ApiItemResponse<IReadOnlyList<DestinoResponseDto>>(destinos));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearDestinoRequestDto request, CancellationToken cancellationToken)
    {
        var destino = await _service.CrearDestinoAsync(request, UsuarioAccion, IpActual, cancellationToken);
        return StatusCode(201, new ApiItemResponse<DestinoResponseDto>(destino, 201));
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarDestinoRequestDto request, CancellationToken cancellationToken)
    {
        var destino = await _service.ActualizarDestinoAsync(guid, request, UsuarioAccion, IpActual, cancellationToken);
        return Ok(new ApiItemResponse<DestinoResponseDto>(destino));
    }

    [HttpDelete("{guid:guid}")]
    public async Task<IActionResult> Eliminar(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarDestinoAsync(guid, UsuarioAccion, IpActual, cancellationToken);
        return NoContent();
    }
}
