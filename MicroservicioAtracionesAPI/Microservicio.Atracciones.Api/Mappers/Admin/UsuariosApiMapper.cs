using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Usuarios;

namespace Microservicio.Atracciones.Api.Mappers.Admin
{
    public static class UsuariosApiMapper
    {
        public static ApiItemResponse<UsuarioResponse> ToResponse(UsuarioResponse u, string msg = "Ok")
            => new() { Status = 200, Message = msg, Data = u };

        public static ApiItemResponse<IReadOnlyList<UsuarioResponse>> ToListResponse(IReadOnlyList<UsuarioResponse> list)
            => new() { Status = 200, Message = "Consulta exitosa", Data = list };
    }
}
