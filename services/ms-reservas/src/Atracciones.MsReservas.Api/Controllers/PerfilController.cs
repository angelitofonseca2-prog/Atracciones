using System.Security.Claims;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Atracciones.MsReservas.DataManagement.Models;
using Atracciones.MsReservas.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsReservas.Api.Controllers;

[ApiController]
[Route("api/v1/clientes/perfil")]
[Authorize(Policy = "ClienteAutenticado")]
[Produces("application/json")]
public sealed class PerfilController : ControllerBase
{
    private readonly IClienteRepository _repo;

    public PerfilController(IClienteRepository repo) => _repo = repo;

    private Guid UsuGuidActual
    {
        get
        {
            var claim = User.FindFirstValue("usu_guid");
            if (!Guid.TryParse(claim, out var g))
                throw new UnauthorizedAccessException("El token no tiene un usuario válido.");
            return g;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken cancellationToken)
    {
        var dto = await _repo.ObtenerActivoPorGuidAsync(UsuGuidActual, cancellationToken);
        if (dto is null)
            return NotFound(new { status = 404, message = "Cliente no encontrado." });

        return Ok(new { status = 200, message = "Consulta exitosa", data = Map(dto) });
    }

    [HttpPut]
    public async Task<IActionResult> Actualizar(
        [FromBody] ActualizarPerfilClienteRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Correo is not null && string.IsNullOrWhiteSpace(request.Correo))
            return BadRequest(new { status = 400, error = "Correo inválido." });

        var dto = await _repo.ActualizarCamposAsync(UsuGuidActual, new ActualizarClienteInternoDto
        {
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Correo = request.Correo,
            Telefono = request.Telefono,
        }, cancellationToken);

        if (dto is null)
            return NotFound(new { status = 404, message = "Cliente no encontrado." });

        return Ok(new { status = 200, message = "Perfil actualizado", data = Map(dto) });
    }

    private static PerfilClienteResponse Map(ClienteDto c) => new()
    {
        CliGuid = c.CliGuid,
        Nombres = c.Nombres,
        Apellidos = c.Apellidos,
        Correo = c.Correo,
        Telefono = c.Telefono,
        TipoIdentificacion = c.TipoIdentificacion,
        NumeroIdentificacion = c.NumeroIdentificacion,
    };
}
