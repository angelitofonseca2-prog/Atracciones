using Microservicio.Atracciones.DataManagement.Models.Seguridad;

namespace Microservicio.Atracciones.Api.Services
{
    public interface TokenService
    {
        (string Token, DateTime Expiracion) GenerarToken(LoginDataModel model);
    }
}
