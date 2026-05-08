using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Public.Resenias;

namespace Microservicio.Atracciones.Api.Mappers.Public
{
    public static class ReseniasApiMapper
    {
        public static ApiItemResponse<ReseniaResponse> ToResponse(
            ReseniaResponse resenia,
            int statusCode = 200,
            string? message = null)
            => new()
            {
                Status = statusCode,
                Message = message ?? (statusCode == 201
                    ? "Reseña registrada exitosamente"
                    : "Operación exitosa"),
                Data = resenia
            };

        public static ApiItemResponse<IReadOnlyList<ReseniaResponse>> ToListResponse(
            IReadOnlyList<ReseniaResponse> resenias)
            => new()
            {
                Status = 200,
                Message = "Consulta exitosa",
                Data = resenias
            };
    }
}
