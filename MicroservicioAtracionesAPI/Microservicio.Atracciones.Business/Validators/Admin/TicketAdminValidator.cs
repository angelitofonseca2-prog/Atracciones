using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;
using Microservicio.Atracciones.Business.Exceptions;

namespace Microservicio.Atracciones.Business.Validators.Admin
{
    public static class TicketAdminValidator
    {
        private static readonly string[] TiposValidos = { "Adulto", "Niño", "Grupo", "Estudiante", "Senior" };

        public static void ValidarCrear(CrearTicketRequest request)
        {
            var errores = new List<string>();

            if (request.AtGuid == Guid.Empty)
                errores.Add("El GUID de la atracción es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Titulo))
                errores.Add("El título del ticket es obligatorio.");

            if (request.Precio < 0)
                errores.Add("El precio no puede ser negativo.");

            if (!TiposValidos.Contains(request.TipoParticipante))
                errores.Add($"TipoParticipante inválido. Valores: {string.Join(", ", TiposValidos)}.");

            if (request.CapacidadMaxima < 1)
                errores.Add("La capacidad máxima debe ser mayor a 0.");

            if (request.CuposDisponibles < 0 || request.CuposDisponibles > request.CapacidadMaxima)
                errores.Add("Los cupos disponibles deben estar entre 0 y la capacidad máxima.");

            if (errores.Any())
                throw new ValidationException(errores);
        }

        public static void ValidarCrearHorario(CrearHorarioRequest request)
        {
            var errores = new List<string>();

            if (request.TckGuid == Guid.Empty)
                errores.Add("El GUID del ticket es obligatorio.");

            if (request.Fecha < DateOnly.FromDateTime(DateTime.UtcNow))
                errores.Add("La fecha del horario no puede ser una fecha pasada.");

            if (request.HoraFin.HasValue && request.HoraFin <= request.HoraInicio)
                errores.Add("La hora de fin debe ser posterior a la hora de inicio.");

            if (request.CuposDisponibles < 0)
                errores.Add("Los cupos disponibles no pueden ser negativos.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
