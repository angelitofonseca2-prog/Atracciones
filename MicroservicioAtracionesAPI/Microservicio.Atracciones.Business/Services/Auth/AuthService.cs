using Microservicio.Atracciones.Business.DTOs.Auth;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Auth;
using Microservicio.Atracciones.Business.Validators;
using Microservicio.Atracciones.DataManagement.Interfaces;

namespace Microservicio.Atracciones.Business.Services.Auth;

/// <summary>
/// Valida credenciales de login y retorna los datos del usuario autenticado.
/// 
/// Lo que NO hace esta clase (y no debe hacer):
///   - Generar JWT
///   - Firmar tokens
///   - Conocer claves secretas
///   - Configurar expiración
/// 
/// Todo eso es responsabilidad de la capa API (AuthenticationExtensions +
/// un servicio de infraestructura registrado allí).
/// Business solo responde: "estas credenciales son válidas, aquí están
/// los datos del usuario con sus roles."
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioDataService _usuarioService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUsuarioDataService usuarioService, IPasswordHasher passwordHasher)
    {
        _usuarioService = usuarioService;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioAutenticadoDto> ValidarCredencialesAsync(LoginRequest request)
    {
        AuthValidator.Validar(request);

        var usuario = await _usuarioService.ObtenerPorLoginAsync(request.Login)
            ?? throw new UnauthorizedBusinessException("Credenciales inválidas.");

        if (usuario.UsuEstado != 'A')
            throw new UnauthorizedBusinessException("El usuario está inactivo.");

        if (!_passwordHasher.Verificar(request.Password, usuario.UsuPasswordHash))
            throw new UnauthorizedBusinessException("Credenciales inválidas.");

        // Retorna los datos del usuario para que la API construya el JWT
        return new UsuarioAutenticadoDto
        {
            UsuId = usuario.UsuId,
            UsuGuid = usuario.UsuGuid,
            Login = usuario.UsuLogin,
            CliId = usuario.CliId, // E-05
            Roles = usuario.Roles.Select(r => r.RolDescripcion.ToUpper()).ToList() // E-06 (por seguridad, upper acá tmb)
        };
    }
}
