using Atracciones.MsReservas.Api.Models;
using Atracciones.MsReservas.Api.Options;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Atracciones.MsReservas.DataManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Atracciones.MsReservas.Api.Controllers.Internal;

[ApiController]
[Route("internal/v1/clientes")]
public sealed class CrmMirrorController : ControllerBase
{
    private readonly IClienteRepository _repo;
    private readonly ClientesMirrorOptions _opts;

    public CrmMirrorController(IClienteRepository repo, IOptions<ClientesMirrorOptions> opts)
    {
        _repo = repo;
        _opts = opts.Value;
    }

    [HttpPost("mirror")]
    public async Task<IActionResult> Mirror([FromBody] CrmMirrorPayload body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_opts.MonolithApiKey) ||
            !Request.Headers.TryGetValue("X-Monolith-Sync-Key", out var sent) ||
            sent.Count != 1 ||
            !string.Equals(sent[0], _opts.MonolithApiKey, StringComparison.Ordinal))
            return Unauthorized();

        await _repo.UpsertMirrorAsync(new ClienteMirrorDto
        {
            UsuGuid = body.UsuGuid,
            TipoIdentificacion = body.TipoIdentificacion,
            NumeroIdentificacion = body.NumeroIdentificacion,
            Nombres = body.Nombres,
            Apellidos = body.Apellidos,
            RazonSocial = body.RazonSocial,
            Correo = body.Correo,
            Telefono = body.Telefono,
            Direccion = body.Direccion,
            CreadoPor = body.CreadoPor,
            IpCreador = body.IpCreador,
        }, cancellationToken);

        return Ok(new { ok = true });
    }
}
