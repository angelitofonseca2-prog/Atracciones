using Microservicio.Atracciones.Business.DTOs.Admin.Destinos;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators.Admin
{
    public static class DestinoAdminValidator
    {
        public static void ValidarCrear(CrearDestinoRequest request)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Nombre))
                errores.Add("El nombre del destino es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Pais))
                errores.Add("El país del destino es obligatorio.");

            if (!string.IsNullOrWhiteSpace(request.ImagenUrl) && !Uri.IsWellFormedUriString(request.ImagenUrl, UriKind.Absolute))
                errores.Add("La URL de imagen debe ser una URL absoluta válida.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
