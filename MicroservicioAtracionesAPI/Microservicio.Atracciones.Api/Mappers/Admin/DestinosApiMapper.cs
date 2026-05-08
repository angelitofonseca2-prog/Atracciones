using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Destinos;

namespace Microservicio.Atracciones.Api.Mappers.Admin
{
    public static class DestinosApiMapper
    {
        public static ApiItemResponse<DestinoResponse> ToResponse(DestinoResponse d, string msg = "Ok")
            => new() { Status = 200, Message = msg, Data = d };

        public static ApiItemResponse<IReadOnlyList<DestinoResponse>> ToListResponse(IReadOnlyList<DestinoResponse> list)
            => new() { Status = 200, Message = "Consulta exitosa", Data = list };
    }
}
