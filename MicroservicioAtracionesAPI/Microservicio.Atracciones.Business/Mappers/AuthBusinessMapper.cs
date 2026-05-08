using Microservicio.Atracciones.Business.DTOs.Auth;

namespace Microservicio.Atracciones.Business.Mappers
{
    public static class AuthBusinessMapper
    {
        public static LoginResponse ToResponse(UsuarioAutenticadoDto usuario, string token, DateTime expiracion)
            => new()
            {
                Login = usuario.Login,
                Roles = usuario.Roles.ToList(),
                Token = token,
                Expiracion = expiracion
            };
    }
}