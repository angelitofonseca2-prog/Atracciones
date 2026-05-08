using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Resenias;

namespace Microservicio.Atracciones.Api.Mappers.Admin
{
    public static class ReseniasAdminApiMapper
    {
        public static ApiItemResponse<ReseniaAdminResponse> ToResponse(ReseniaAdminResponse r, string msg = "Ok")
            => new() { Status = 200, Message = msg, Data = r };

        public static ApiItemResponse<IReadOnlyList<ReseniaAdminResponse>> ToListResponse(
            IReadOnlyList<ReseniaAdminResponse> list)
            => new() { Status = 200, Message = "Consulta exitosa", Data = list };
    }
}
