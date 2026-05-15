using Atracciones.MsIdentidad.Business.DTOs;
using Atracciones.MsIdentidad.Business.Exceptions;

namespace Atracciones.MsIdentidad.Business.Validators;

public static class AuthValidator
{
    public static void Validar(LoginRequest request)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Login))
            errores.Add("El login es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Password))
            errores.Add("La contraseña es obligatoria.");
        if (errores.Count > 0)
            throw new ValidationException(errores);
    }
}
