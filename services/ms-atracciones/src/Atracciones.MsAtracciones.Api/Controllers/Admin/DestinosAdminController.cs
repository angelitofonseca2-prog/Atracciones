using System.Security.Claims;
using Atracciones.MsAtracciones.Business.Dtos.Admin.Catalogos;
using Atracciones.MsAtracciones.Business.Services;
using Atracciones.MsAtracciones.Api.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsAtracciones.Api.Controllers.Admin;

[ApiController]
[Route("api/v2/admin/destinos")]
[Authorize(Policy = "SoloAdmin")]
[Produces("application/json")]
public sealed class DestinosAdminController : ControllerBase
{
    private readonly ICatalogosAdminAppService _service;

    public DestinosAdminController(ICatalogosAdminAppService service) => _service = service;

    private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
    private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<IReadOnlyList<DestinoResponseDto>>(await _service.ListDestinosAsync(cancellationToken)));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearDestinoRequestDto request, CancellationToken cancellationToken)
        => StatusCode(201, new ApiItemResponse<DestinoResponseDto>(await _service.CrearDestinoAsync(request, UsuarioAccion, IpActual, cancellationToken), 201));

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarDestinoRequestDto request, CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<DestinoResponseDto>(await _service.ActualizarDestinoAsync(guid, request, UsuarioAccion, IpActual, cancellationToken)));

    [HttpDelete("{guid:guid}")]
    public async Task<IActionResult> Eliminar(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarDestinoAsync(guid, UsuarioAccion, IpActual, cancellationToken);
        return NoContent();
    }
}
