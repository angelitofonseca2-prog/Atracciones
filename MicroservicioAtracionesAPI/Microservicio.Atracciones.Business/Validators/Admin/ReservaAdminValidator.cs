using Microservicio.Atracciones.Business.DTOs.Admin.Reservas;
using Microservicio.Atracciones.Business.Exceptions;
namespace Microservicio.Atracciones.Business.Validators.Admin
{
    public static class ReservaAdminValidator
    {
        public static void ValidarActualizarEstado(ActualizarEstadoReservaRequest request)
        {
            var errores = new List<string>();

            if (request.NuevoEstado != 'A' && request.NuevoEstado != 'I' && request.NuevoEstado != 'C')
                errores.Add("Estado inválido. Valores aceptados: A (Activa), I (Inactiva), C (Cancelada).");

            if (request.NuevoEstado == 'C' && string.IsNullOrWhiteSpace(request.Motivo))
                errores.Add("El motivo es obligatorio para cancelar una reserva.");

            if (errores.Any())
                throw new ValidationException(errores);
        }
    }
}
