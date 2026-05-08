using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators.Admin
{
    public static class FacturaAdminValidator
    {
        public static void ValidarGenerar(GenerarFacturaRequest request)
        {
            var errores = new List<string>();

            if (request.RevGuid == Guid.Empty)
                errores.Add("El GUID de la reserva es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.NombreReceptor))
                errores.Add("El nombre del receptor es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.CorreoReceptor))
                errores.Add("El correo del receptor es obligatorio.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
