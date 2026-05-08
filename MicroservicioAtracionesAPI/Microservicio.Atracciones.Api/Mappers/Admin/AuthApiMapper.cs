using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Auth;

namespace Microservicio.Atracciones.Api.Mappers.Admin
{
    public static class AuthApiMapper
    {
        public static ApiItemResponse<LoginResponse> ToResponse(LoginResponse login)
            => new() { Status = 200, Message = "Autenticación exitosa", Data = login };
    }
}
