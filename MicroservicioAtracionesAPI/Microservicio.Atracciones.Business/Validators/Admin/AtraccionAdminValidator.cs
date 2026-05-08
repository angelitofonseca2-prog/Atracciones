using Microservicio.Atracciones.Business.DTOs.Admin.Atracciones;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators.Admin
{
    public static class AtraccionAdminValidator
    {
        public static void ValidarCrear(CrearAtraccionRequest request)
        {
            var errores = new List<string>();

            if (request.DestinoGuid == Guid.Empty)
                errores.Add("El GUID del destino es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Nombre))
                errores.Add("El nombre de la atracción es obligatorio.");

            if (request.DuracionMinutos.HasValue && request.DuracionMinutos < 1)
                errores.Add("La duración en minutos debe ser mayor a 0.");

            if (request.PrecioReferencia.HasValue && request.PrecioReferencia < 0)
                errores.Add("El precio de referencia no puede ser negativo.");

            ValidarRelacionObligatoria(request.CategoriaGuids, "Debe incluir al menos una categoría.");
            ValidarRelacionObligatoria(request.IdiomaGuids, "Debe incluir al menos un idioma.");
            ValidarRelacionObligatoria(request.ImagenGuids, "Debe incluir al menos una imagen.");
            ValidarRelacionObligatoria(request.IncluyeGuids, "Debe incluir al menos un elemento incluido.");

            ValidarGuids(request.CategoriaGuids, "categoría", errores);
            ValidarGuids(request.IdiomaGuids, "idioma", errores);
            ValidarGuids(request.ImagenGuids, "imagen", errores);
            ValidarGuids(request.IncluyeGuids, "incluye", errores);

            if (errores.Any())
                throw new ValidationException(errores);
        }

        public static void ValidarActualizar(ActualizarAtraccionRequest request)
        {
            var errores = new List<string>();

            if (request.DestinoGuid.HasValue && request.DestinoGuid.Value == Guid.Empty)
                errores.Add("El GUID del destino no puede estar vacío.");

            if (request.Nombre is not null && string.IsNullOrWhiteSpace(request.Nombre))
                errores.Add("El nombre de la atracción no puede estar vacío.");

            if (request.DuracionMinutos.HasValue && request.DuracionMinutos < 1)
                errores.Add("La duración en minutos debe ser mayor a 0.");

            if (request.PrecioReferencia.HasValue && request.PrecioReferencia < 0)
                errores.Add("El precio de referencia no puede ser negativo.");

            ValidarRelacionSiSeEnvia(request.CategoriaGuids, "categoría", "No se puede enviar una lista vacía de categorías.", errores);
            ValidarRelacionSiSeEnvia(request.IdiomaGuids, "idioma", "No se puede enviar una lista vacía de idiomas.", errores);
            ValidarRelacionSiSeEnvia(request.ImagenGuids, "imagen", "No se puede enviar una lista vacía de imágenes.", errores);
            ValidarRelacionSiSeEnvia(request.IncluyeGuids, "incluye", "No se puede enviar una lista vacía de elementos incluidos.", errores);

            if (errores.Any())
                throw new ValidationException(errores);
        }

        private static void ValidarRelacionObligatoria(IList<Guid> guids, string mensaje)
        {
            if (guids is null || guids.Count == 0)
                throw new ValidationException(new[] { mensaje });
        }

        private static void ValidarRelacionSiSeEnvia(IList<Guid>? guids, string nombre, string mensajeVacio, IList<string> errores)
        {
            if (guids is null) return;
            if (guids.Count == 0)
                errores.Add(mensajeVacio);
            ValidarGuids(guids, nombre, errores);
        }

        private static void ValidarGuids(IList<Guid> guids, string nombre, IList<string> errores)
        {
            if (guids.Any(g => g == Guid.Empty))
                errores.Add($"Cada GUID de {nombre} debe ser válido.");

            if (guids.Distinct().Count() != guids.Count)
                errores.Add($"No se puede repetir el mismo GUID de {nombre}.");
        }
    }
}
