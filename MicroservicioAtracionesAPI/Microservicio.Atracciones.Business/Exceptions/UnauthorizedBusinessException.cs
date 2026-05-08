
namespace Microservicio.Atracciones.Business.Exceptions
{
    public class UnauthorizedBusinessException : BusinessException
    {
        public UnauthorizedBusinessException(string message = "Credenciales inválidas o sesión expirada.")
            : base(message, 401)
        {
        }
    }

}
