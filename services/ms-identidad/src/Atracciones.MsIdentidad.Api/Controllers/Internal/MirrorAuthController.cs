using Atracciones.MsIdentidad.Api.Models;
using Atracciones.MsIdentidad.Api.Options;
using Atracciones.MsIdentidad.Business.Auth;
using Atracciones.MsIdentidad.Business.DTOs;
using Atracciones.MsIdentidad.DataManagement.Interfaces;
using Atracciones.MsIdentidad.DataManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Atracciones.MsIdentidad.Api.Controllers.Internal;

[ApiController]
[Route("internal/v1/auth")]
public sealed class MirrorAuthController : ControllerBase
{
    private readonly IIdentidadUsuarioRepository _usuarios;
    private readonly IJwtTokenIssuer _jwtTokenIssuer;
    private readonly InternalSyncOptions _sync;

    public MirrorAuthController(
        IIdentidadUsuarioRepository usuarios,
        IJwtTokenIssuer jwtTokenIssuer,
        IOptions<InternalSyncOptions> sync)
    {
        _usuarios = usuarios;
        _jwtTokenIssuer = jwtTokenIssuer;
        _sync = sync.Value;
    }

    /// <summary>
    /// Sincroniza credenciales desde el monolito y devuelve un JWT RS256 (mismo contrato que login).
    /// </summary>
    [HttpPost("mirror")]
    [ProducesResponseType(typeof(ApiItemResponse<LoginResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<IActionResult> Mirror([FromBody] MirrorRequest body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_sync.MonolithApiKey) ||
            !Request.Headers.TryGetValue("X-Monolith-Sync-Key", out var sent) ||
            sent.Count != 1 ||
            !string.Equals(sent[0], _sync.MonolithApiKey, StringComparison.Ordinal))
        {
            return Unauthorized();
        }

        var dto = new MonolithUsuarioEspejoDto
        {
            UsuId = body.UsuId,
            UsuGuid = body.UsuGuid,
            Login = body.Login.Trim(),
            PasswordHash = body.PasswordHash,
            CliId = body.CliId,
            Roles = body.Roles.Count > 0
                ? body.Roles
                : new List<string> { "CLIENTE" },
        };

        await _usuarios.UpsertEspejoMonolithAsync(dto, cancellationToken);

        var rolesNormalizados = dto.Roles.Select(r => r.Trim().ToUpperInvariant()).ToList();
        var (token, expiracion) = _jwtTokenIssuer.Emitir(new UsuarioParaToken(
            body.UsuId,
            body.UsuGuid,
            body.Login.Trim(),
            body.CliId,
            rolesNormalizados));

        return Ok(new ApiItemResponse<LoginResponse>(new LoginResponse
        {
            Token = token,
            Expiracion = expiracion,
            Login = body.Login.Trim(),
            Roles = rolesNormalizados,
        }));
    }
}
