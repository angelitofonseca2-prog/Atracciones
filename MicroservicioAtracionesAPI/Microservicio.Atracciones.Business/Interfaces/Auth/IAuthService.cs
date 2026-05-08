using Microservicio.Atracciones.Business.DTOs.Auth;

namespace Microservicio.Atracciones.Business.Interfaces.Auth
{
    public interface IAuthService
    {
        /// <summary>
        /// Valida login y password. Si son correctos retorna los datos del
        /// usuario autenticado para que la API genere el token.
        /// Lanza UnauthorizedBusinessException si las credenciales son inválidas.
        /// </summary>
        Task<UsuarioAutenticadoDto> ValidarCredencialesAsync(LoginRequest request);
    }
    public interface IPasswordHasher
    {
        /// <summary>
        /// Genera el hash de una contraseña en texto plano.
        /// Se usa al crear o actualizar un usuario.
        /// </summary>
        string Hashear(string password);

        /// <summary>
        /// Verifica que una contraseña en texto plano coincide con su hash.
        /// Se usa en el login.
        /// </summary>
        bool Verificar(string password, string hash);
    }
}
