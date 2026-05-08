using Microservicio.Atracciones.Business.DTOs.Public.Resenias;
using Microservicio.Atracciones.Business.Exceptions;


namespace Microservicio.Atracciones.Business.Validators.Public
{
    public static class ReseniaPublicValidator
    {
        public static void Validar(CrearReseniaRequest request)
        {
            var errores = new List<string>();

            if (request.RevGuid == Guid.Empty)
                errores.Add("El GUID de la reserva es obligatorio.");

            if (request.Rating < 1 || request.Rating > 5)
                errores.Add("El rating debe estar entre 1 y 5.");

            if (request.Comentario?.Length > 1000)
                errores.Add("El comentario no puede superar 1000 caracteres.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
