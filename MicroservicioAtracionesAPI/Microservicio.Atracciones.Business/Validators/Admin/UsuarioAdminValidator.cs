using Microservicio.Atracciones.Business.DTOs.Admin.Usuarios;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators.Admin
{
    public static class UsuarioAdminValidator
    {
        public static void ValidarCrear(CrearUsuarioRequest request)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Login))
                errores.Add("El login es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
                errores.Add("La contraseña debe tener al menos 8 caracteres.");

            if (request.Roles is null || !request.Roles.Any())
                errores.Add("Debe asignar al menos un rol.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
