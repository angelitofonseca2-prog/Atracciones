using Atracciones.MsIdentidad.DataManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsIdentidad.Api.Controllers;

/// <summary>Listado administrativo de usuarios (auth.*). Expuesto vía gateway en /api/v2/admin/usuarios.</summary>
[ApiController]
[Route("api/v2/admin/usuarios")]
[Authorize(Policy = "SoloAdmin")]
[Produces("application/json")]
public sealed class AdminUsuariosController : ControllerBase
{
    private readonly IIdentidadUsuarioRepository _repo;

    public AdminUsuariosController(IIdentidadUsuarioRepository repo) => _repo = repo;

    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var (items, total) = await _repo.ListarParaAdminAsync(page, limit, ct);
        var data = items.Select(i => new
        {
            usu_guid = i.UsuGuid.ToString("D"),
            usr_guid = i.UsuGuid.ToString("D"),
            login = i.Login,
            estado = i.Estado.ToString(),
            roles = i.Roles,
            fecha_registro = i.FechaRegistro,
        }).ToList();

        return Ok(new
        {
            status = 200,
            message = "Consulta exitosa",
            data,
            pagination = new { total, page, limit },
        });
    }
}
