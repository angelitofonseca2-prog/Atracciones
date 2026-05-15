using Atracciones.MsIdentidad.Business.Auth;
using Atracciones.MsIdentidad.Business.DTOs;
using Atracciones.MsIdentidad.Business.Exceptions;
using Atracciones.MsIdentidad.Business.Interfaces;
using Atracciones.MsIdentidad.Business.Validators;
using Atracciones.MsIdentidad.DataManagement.Interfaces;

namespace Atracciones.MsIdentidad.Business.Services;

public sealed class AuthService : IAuthService
{
    private readonly IIdentidadUsuarioRepository _usuarios;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IIdentidadUsuarioRepository usuarios, IPasswordHasher passwordHasher)
    {
        _usuarios = usuarios;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioAutenticadoDto> ValidarCredencialesAsync(LoginRequest request, CancellationToken ct = default)
    {
        AuthValidator.Validar(request);

        var usuario = await _usuarios.ObtenerActivoPorLoginAsync(request.Login, ct)
            ?? throw new UnauthorizedBusinessException("Credenciales inválidas.");

        if (!_passwordHasher.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedBusinessException("Credenciales inválidas.");

        var roles = await _usuarios.ListarRolesPorUsuIdAsync(usuario.UsuId, ct);

        return new UsuarioAutenticadoDto
        {
            UsuId = usuario.UsuId,
            UsuGuid = usuario.UsuGuid,
            Login = usuario.Login,
            CliId = usuario.CliId,
            Roles = roles.Select(r => r.ToUpperInvariant()).ToList(),
        };
    }
}
