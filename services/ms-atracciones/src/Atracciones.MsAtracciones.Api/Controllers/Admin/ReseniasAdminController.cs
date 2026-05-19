using Atracciones.MsAtracciones.Api.Models.Common;
using Atracciones.MsAtracciones.Business.Dtos.Admin.Resenias;
using Atracciones.MsAtracciones.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Atracciones.MsAtracciones.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/resenias")]
[Authorize(Policy = "SoloAdmin")]
[Produces("application/json")]
public sealed class ReseniasAdminController : ControllerBase
{
    private readonly IReseniaAdminAppService _service;

    public ReseniasAdminController(IReseniaAdminAppService service) => _service = service;

    private string UsuarioAccion => User.FindFirstValue("login") ?? User.FindFirstValue(ClaimTypes.Name) ?? "sistema";
    private string IpActual => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";

    [HttpGet]
    [ProducesResponseType(typeof(ApiItemResponse<IReadOnlyList<ReseniaAdminResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> Listar([FromQuery] Guid atraccionGuid, CancellationToken ct)
    {
        var resenias = await _service.ListarPorAtraccionAsync(atraccionGuid, ct);
        return Ok(new ApiItemResponse<IReadOnlyList<ReseniaAdminResponse>>(resenias));
    }

    [HttpGet("{guid:guid}")]
    [ProducesResponseType(typeof(ApiItemResponse<ReseniaAdminResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> ObtenerPorGuid(Guid guid, CancellationToken ct)
    {
        var resenia = await _service.ObtenerPorGuidAsync(guid, ct);
        return Ok(new ApiItemResponse<ReseniaAdminResponse>(resenia));
    }

    [HttpPut("{guid:guid}")]
    [ProducesResponseType(typeof(ApiItemResponse<ReseniaAdminResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> Actualizar(Guid guid, [FromBody] ActualizarReseniaAdminRequest request, CancellationToken ct)
    {
        var resenia = await _service.ActualizarAsync(guid, request, UsuarioAccion, IpActual, ct);
        return Ok(new ApiItemResponse<ReseniaAdminResponse>(resenia));
    }

    [HttpDelete("{guid:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> Eliminar(Guid guid, CancellationToken ct)
    {
        await _service.EliminarAsync(guid, UsuarioAccion, IpActual, ct);
        return NoContent();
    }
}
