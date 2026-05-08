using Microservicio.Atracciones.Business.DTOs.Public.Atracciones;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators.Public
{
    public static class AtraccionPublicValidator
    {
        private static readonly string[] HorariosValidos = { "05:00-12:00", "12:00-18:00", "18:00-05:00" };
        private static readonly string[] OrdenamientoValido = { "trending", "lowest_price", "highest_weighted_rating" };

        public static void Validar(AtraccionFiltroRequest request)
        {
            var errores = new List<string>();

            if (request.Page < 1)
                errores.Add("El campo 'page' debe ser mayor a 0.");

            if (request.Limit < 1 || request.Limit > 50)
                errores.Add("El campo 'limit' debe estar entre 1 y 50.");

            if (!string.IsNullOrWhiteSpace(request.Horario) && !HorariosValidos.Contains(request.Horario))
                errores.Add($"El campo 'horario' solo acepta: {string.Join(", ", HorariosValidos)}.");

            if (!OrdenamientoValido.Contains(request.OrdenarPor))
                errores.Add($"El campo 'ordenar_por' solo acepta: {string.Join(", ", OrdenamientoValido)}.");

            if (request.CalificacionMin.HasValue && (request.CalificacionMin < 0 || request.CalificacionMin > 5))
                errores.Add("El campo 'calificacion_min' debe estar entre 0 y 5.");

            if (request.Tipo is not null && !Guid.TryParse(request.Tipo, out _))
                errores.Add("El campo 'tipo' debe ser un UUID válido.");

            if (request.Subtipo is not null && !Guid.TryParse(request.Subtipo, out _))
                errores.Add("El campo 'subtipo' debe ser un UUID válido.");

            if (errores.Any())
                throw new ValidationException(errores);
        }

        public static void ValidarGuid(string guid)
        {
            if (!Guid.TryParse(guid, out _))
                throw new ValidationException($"El identificador '{guid}' no es un UUID válido.");
        }

        public static void ValidarCiudadFiltros(string? ciudad)
        {
            if (ciudad is not null && string.IsNullOrWhiteSpace(ciudad))
                throw new ValidationException("El parámetro 'ciudad' no puede estar vacío.");
        }
    }
}
