using System.Security.Claims;
using Atracciones.MsAtracciones.Business.Dtos.Admin.Catalogos;
using Atracciones.MsAtracciones.Business.Services;
using Atracciones.MsAtracciones.Api.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsAtracciones.Api.Controllers.Admin;

[ApiController]
[Route("api/v2/admin")]
[Authorize(Policy = "SoloAdmin")]
[Produces("application/json")]
public sealed class CatalogosAdminController : ControllerBase
{
    private readonly ICatalogosAdminAppService _service;

    public CatalogosAdminController(ICatalogosAdminAppService service) => _service = service;

    private string UsuarioAccion => User.FindFirstValue("login") ?? "sistema";
    private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    [HttpGet("categorias")]
    public async Task<IActionResult> ListarCategorias(CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<IReadOnlyList<CategoriaResponseDto>>(await _service.ListCategoriasAsync(cancellationToken)));

    [HttpPost("categorias")]
    public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaRequestDto request, CancellationToken cancellationToken)
        => StatusCode(201, new ApiItemResponse<CategoriaResponseDto>(await _service.CrearCategoriaAsync(request, UsuarioAccion, IpActual, cancellationToken), 201));

    [HttpPut("categorias/{guid:guid}")]
    public async Task<IActionResult> ActualizarCategoria(Guid guid, [FromBody] ActualizarCategoriaRequestDto request, CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<CategoriaResponseDto>(await _service.ActualizarCategoriaAsync(guid, request, UsuarioAccion, IpActual, cancellationToken)));

    [HttpDelete("categorias/{guid:guid}")]
    public async Task<IActionResult> EliminarCategoria(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarCategoriaAsync(guid, UsuarioAccion, IpActual, cancellationToken);
        return NoContent();
    }

    [HttpGet("idiomas")]
    public async Task<IActionResult> ListarIdiomas(CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<IReadOnlyList<IdiomaResponseDto>>(await _service.ListIdiomasAsync(cancellationToken)));

    [HttpPost("idiomas")]
    public async Task<IActionResult> CrearIdioma([FromBody] CrearIdiomaRequestDto request, CancellationToken cancellationToken)
        => StatusCode(201, new ApiItemResponse<IdiomaResponseDto>(await _service.CrearIdiomaAsync(request, UsuarioAccion, IpActual, cancellationToken), 201));

    [HttpPut("idiomas/{guid:guid}")]
    public async Task<IActionResult> ActualizarIdioma(Guid guid, [FromBody] ActualizarIdiomaRequestDto request, CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<IdiomaResponseDto>(await _service.ActualizarIdiomaAsync(guid, request, UsuarioAccion, IpActual, cancellationToken)));

    [HttpDelete("idiomas/{guid:guid}")]
    public async Task<IActionResult> EliminarIdioma(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarIdiomaAsync(guid, UsuarioAccion, IpActual, cancellationToken);
        return NoContent();
    }

    [HttpGet("incluye")]
    public async Task<IActionResult> ListarIncluye(CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<IReadOnlyList<IncluyeResponseDto>>(await _service.ListIncluyeAsync(cancellationToken)));

    [HttpPost("incluye")]
    public async Task<IActionResult> CrearIncluye([FromBody] CrearIncluyeRequestDto request, CancellationToken cancellationToken)
        => StatusCode(201, new ApiItemResponse<IncluyeResponseDto>(await _service.CrearIncluyeAsync(request, cancellationToken), 201));

    [HttpPut("incluye/{guid:guid}")]
    public async Task<IActionResult> ActualizarIncluye(Guid guid, [FromBody] ActualizarIncluyeRequestDto request, CancellationToken cancellationToken)
        => Ok(new ApiItemResponse<IncluyeResponseDto>(await _service.ActualizarIncluyeAsync(guid, request, cancellationToken)));

    [HttpDelete("incluye/{guid:guid}")]
    public async Task<IActionResult> EliminarIncluye(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarIncluyeAsync(guid, cancellationToken);
        return NoContent();
    }
}
