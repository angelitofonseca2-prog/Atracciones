using System.Security.Claims;
using Atracciones.MsCatalogos.Api.Models;
using Atracciones.MsCatalogos.Business;
using Atracciones.MsCatalogos.Business.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsCatalogos.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin")]
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
    {
        var response = await _service.ListCategoriasAsync(cancellationToken);
        return Ok(new ApiItemResponse<IReadOnlyList<CategoriaResponseDto>>(response));
    }

    [HttpPost("categorias")]
    public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _service.CrearCategoriaAsync(request, UsuarioAccion, IpActual, cancellationToken);
        return StatusCode(201, new ApiItemResponse<CategoriaResponseDto>(response, 201));
    }

    [HttpPut("categorias/{guid:guid}")]
    public async Task<IActionResult> ActualizarCategoria(Guid guid, [FromBody] ActualizarCategoriaRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _service.ActualizarCategoriaAsync(guid, request, UsuarioAccion, IpActual, cancellationToken);
        return Ok(new ApiItemResponse<CategoriaResponseDto>(response));
    }

    [HttpDelete("categorias/{guid:guid}")]
    public async Task<IActionResult> EliminarCategoria(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarCategoriaAsync(guid, UsuarioAccion, IpActual, cancellationToken);
        return NoContent();
    }

    [HttpGet("idiomas")]
    public async Task<IActionResult> ListarIdiomas(CancellationToken cancellationToken)
    {
        var response = await _service.ListIdiomasAsync(cancellationToken);
        return Ok(new ApiItemResponse<IReadOnlyList<IdiomaResponseDto>>(response));
    }

    [HttpPost("idiomas")]
    public async Task<IActionResult> CrearIdioma([FromBody] CrearIdiomaRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _service.CrearIdiomaAsync(request, UsuarioAccion, IpActual, cancellationToken);
        return StatusCode(201, new ApiItemResponse<IdiomaResponseDto>(response, 201));
    }

    [HttpPut("idiomas/{guid:guid}")]
    public async Task<IActionResult> ActualizarIdioma(Guid guid, [FromBody] ActualizarIdiomaRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _service.ActualizarIdiomaAsync(guid, request, UsuarioAccion, IpActual, cancellationToken);
        return Ok(new ApiItemResponse<IdiomaResponseDto>(response));
    }

    [HttpDelete("idiomas/{guid:guid}")]
    public async Task<IActionResult> EliminarIdioma(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarIdiomaAsync(guid, UsuarioAccion, IpActual, cancellationToken);
        return NoContent();
    }

    [HttpGet("incluye")]
    public async Task<IActionResult> ListarIncluye(CancellationToken cancellationToken)
    {
        var response = await _service.ListIncluyeAsync(cancellationToken);
        return Ok(new ApiItemResponse<IReadOnlyList<IncluyeResponseDto>>(response));
    }

    [HttpPost("incluye")]
    public async Task<IActionResult> CrearIncluye([FromBody] CrearIncluyeRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _service.CrearIncluyeAsync(request, cancellationToken);
        return StatusCode(201, new ApiItemResponse<IncluyeResponseDto>(response, 201));
    }

    [HttpPut("incluye/{guid:guid}")]
    public async Task<IActionResult> ActualizarIncluye(Guid guid, [FromBody] ActualizarIncluyeRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _service.ActualizarIncluyeAsync(guid, request, cancellationToken);
        return Ok(new ApiItemResponse<IncluyeResponseDto>(response));
    }

    [HttpDelete("incluye/{guid:guid}")]
    public async Task<IActionResult> EliminarIncluye(Guid guid, CancellationToken cancellationToken)
    {
        await _service.EliminarIncluyeAsync(guid, cancellationToken);
        return NoContent();
    }
}
