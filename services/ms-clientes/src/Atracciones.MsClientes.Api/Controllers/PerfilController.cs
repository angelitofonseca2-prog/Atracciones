using Atracciones.MsClientes.Business.DTOs;
using Atracciones.MsClientes.Business.Exceptions;
using Atracciones.MsClientes.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Atracciones.MsClientes.Api.Controllers;

[ApiController]
[Route("api/v1/clientes/perfil")]
[Authorize(Policy = "ClienteAutenticado")]
[Produces("application/json")]
public sealed class PerfilController : ControllerBase
{
    private readonly IClientePerfilAppService _service;

    public PerfilController(IClientePerfilAppService service) => _service = service;

    private Guid UsuGuidActual
    {
        get
        {
            var claim = User.FindFirstValue("usu_guid");
            if (!Guid.TryParse(claim, out var g))
                throw new UnauthorizedBusinessException("El token no tiene un usuario válido.");
            return g;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(CancellationToken cancellationToken)
        => Ok(await _service.ObtenerAsync(UsuGuidActual, cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Actualizar([FromBody] ActualizarPerfilClienteRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ActualizarAsync(UsuGuidActual, request, cancellationToken));
}
