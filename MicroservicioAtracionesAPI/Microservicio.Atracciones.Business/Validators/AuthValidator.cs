using Microservicio.Atracciones.Business.DTOs.Auth;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators
{
    public static class AuthValidator
    {
        public static void Validar(LoginRequest request)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Login))
                errores.Add("El login es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password))
                errores.Add("La contraseña es obligatoria.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
