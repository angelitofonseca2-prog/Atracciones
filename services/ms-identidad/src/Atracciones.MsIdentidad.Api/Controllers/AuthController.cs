using Atracciones.MsIdentidad.Api.Models;
using Atracciones.MsIdentidad.Business.Auth;
using Atracciones.MsIdentidad.Business.DTOs;
using Atracciones.MsIdentidad.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Atracciones.MsIdentidad.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenIssuer _jwtTokenIssuer;

    public AuthController(IAuthService authService, IJwtTokenIssuer jwtTokenIssuer)
    {
        _authService = authService;
        _jwtTokenIssuer = jwtTokenIssuer;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiItemResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var userDto = await _authService.ValidarCredencialesAsync(request, cancellationToken);

        var (token, expiracion) = _jwtTokenIssuer.Emitir(new UsuarioParaToken(
            userDto.UsuId,
            userDto.UsuGuid,
            userDto.Login,
            userDto.CliId,
            userDto.Roles));

        var response = new ApiItemResponse<LoginResponse>(new LoginResponse
        {
            Token = token,
            Expiracion = expiracion,
            Login = userDto.Login,
            Roles = userDto.Roles.ToList(),
        });

        return Ok(response);
    }
}
