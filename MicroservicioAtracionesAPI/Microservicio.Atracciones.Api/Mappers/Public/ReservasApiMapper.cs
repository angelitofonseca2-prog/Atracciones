using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Public.Reservas;

namespace Microservicio.Atracciones.Api.Mappers.Public
{
    public static class ReservasApiMapper
    {
        /// <summary>
        /// El campo <see cref="ApiItemResponse{T}.Status"/> del envelope debe coincidir con el código HTTP
        /// (201 en creación, 200 en consulta).
        /// </summary>
        public static ApiItemResponse<ReservaResponse> ToResponse(
            ReservaResponse reserva,
            int statusCode = 200,
            string? message = null)
            => new()
            {
                Status = statusCode,
                Message = message ?? (statusCode == 201
                    ? "Reserva creada exitosamente"
                    : "Reserva obtenida exitosamente"),
                Data = reserva
            };
    }
}
