using Microservicio.Atracciones.Business.DTOs.Public.Reservas;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators.Public
{
    public static class ReservaPublicValidator
    {
        public static void Validar(CrearReservaRequest request)
        {
            var errores = new List<string>();

            if (request.HorGuid == Guid.Empty)
                errores.Add("El GUID del horario es obligatorio.");

            if (request.AtGuid == Guid.Empty)
                errores.Add("El GUID de la atracción es obligatorio.");

            if (request.Lineas is null || !request.Lineas.Any())
                errores.Add("Debe incluir al menos una línea de ticket.");

            foreach (var linea in request.Lineas ?? new List<ReservaDetalleRequest>())
            {
                if (linea.TckGuid == Guid.Empty)
                    errores.Add("Cada línea debe tener un GUID de ticket válido.");

                if (linea.Cantidad < 1)
                    errores.Add($"La cantidad del ticket '{linea.TckGuid}' debe ser mayor a 0.");
            }

            // Verificar que no haya GUIDs de ticket duplicados en las líneas
            var guids = request.Lineas?.Select(l => l.TckGuid).ToList() ?? new List<Guid>();
            if (guids.Distinct().Count() != guids.Count)
                errores.Add("No se puede repetir el mismo tipo de ticket en más de una línea.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
