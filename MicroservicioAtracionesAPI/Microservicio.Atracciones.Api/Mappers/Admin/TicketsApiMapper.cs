using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Tickets;

namespace Microservicio.Atracciones.Api.Mappers.Admin
{
    public static class TicketsApiMapper
    {
        public static ApiItemResponse<TicketResponse> ToResponse(TicketResponse t, string msg = "Ok")
            => new() { Status = 200, Message = msg, Data = t };
    }
}
