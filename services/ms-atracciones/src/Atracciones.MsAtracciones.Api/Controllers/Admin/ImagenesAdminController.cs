using System.Security.Claims;
using Atracciones.MsAtracciones.Business.Dtos.Admin.Catalogos;
using Atracciones.MsAtracciones.Business.Services;
using Atracciones.MsAtracciones.Api.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsAtracciones.Api.Controllers.Admin;

[ApiController]
[Route("api/v2/admin/imagenes")]
[Authorize(Policy = "SoloAdmin")]
[Produces("application/json")]
public sealed class ImagenesAdminController : ControllerBase
{
    private readonly ICatalogosAdminAppService _service;

    public ImagenesAdminController(ICatalogosAdminAppService service) => _service = service;

    private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
    private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<IReadOnlyList<ImagenResponseDto>>(await _service.ListImagenesAsync(cancellationToken)));

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearImagenRequestDto request, CancellationToken cancellationToken)
        => StatusCode(201, new ApiItemResponse<ImagenResponseDto>(await _service.CrearImagenAsync(request, UsuarioAccion, IpActual, cancellationToken), 201));

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarImagenRequestDto request, CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<ImagenResponseDto>(await _service.ActualizarImagenAsync(guid, request, UsuarioAccion, IpActual, cancellationToken)));

    [HttpDelete("{guid:guid}")]
    public async Task<IActionResult> Eliminar(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarImagenAsync(guid, UsuarioAccion, IpActual, cancellationToken);
        return NoContent();
    }
}
