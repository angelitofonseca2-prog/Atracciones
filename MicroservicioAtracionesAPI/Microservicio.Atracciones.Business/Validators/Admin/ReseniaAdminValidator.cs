using Microservicio.Atracciones.Business.DTOs.Admin.Resenias;
using Microservicio.Atracciones.Business.Exceptions;
namespace Microservicio.Atracciones.Business.Validators.Admin
{
    public static class ReseniaAdminValidator
    {
        public static void ValidarActualizar(ActualizarReseniaRequest request)
        {
            var errores = new List<string>();

            if (request.Rating.HasValue && (request.Rating < 1 || request.Rating > 5))
                errores.Add("El rating debe estar entre 1 y 5.");

            if (request.Comentario?.Length > 1000)
                errores.Add("El comentario no puede superar 1000 caracteres.");

            if (request.Estado.HasValue && request.Estado != 'A' && request.Estado != 'I')
                errores.Add("Estado inválido. Valores aceptados: A o I.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
