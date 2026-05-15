using System.Security.Claims;
using Atracciones.MsCatalogos.Api.Models;
using Atracciones.MsCatalogos.Business;
using Atracciones.MsCatalogos.Business.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsCatalogos.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/imagenes")]
[Authorize(Policy = "SoloAdmin")]
[Produces("application/json")]
public sealed class ImagenesController : ControllerBase
{
    private readonly ICatalogosAdminAppService _service;

    public ImagenesController(ICatalogosAdminAppService service) => _service = service;

    private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
    private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var imagenes = await _service.ListImagenesAsync(cancellationToken);
        return Ok(new ApiItemResponse<IReadOnlyList<ImagenResponseDto>>(imagenes));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearImagenRequestDto request, CancellationToken cancellationToken)
    {
        var imagen = await _service.CrearImagenAsync(request, UsuarioAccion, IpActual, cancellationToken);
        return StatusCode(201, new ApiItemResponse<ImagenResponseDto>(imagen, 201));
    }

    [HttpPut("{guid:guid}")]
    public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarImagenRequestDto request, CancellationToken cancellationToken)
    {
        var imagen = await _service.ActualizarImagenAsync(guid, request, UsuarioAccion, IpActual, cancellationToken);
        return Ok(new ApiItemResponse<ImagenResponseDto>(imagen));
    }

    [HttpDelete("{guid:guid}")]
    public async Task<IActionResult> Eliminar(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarImagenAsync(guid, UsuarioAccion, IpActual, cancellationToken);
        return NoContent();
    }
}
