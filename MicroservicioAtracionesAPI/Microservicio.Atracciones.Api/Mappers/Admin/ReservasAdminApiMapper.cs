using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Reservas;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Api.Mappers.Admin
{
    public static class ReservasAdminApiMapper
    {
        public static ApiItemResponse<ReservaAdminResponse> ToResponse(ReservaAdminResponse r)
            => new() { Status = 200, Message = "Consulta exitosa", Data = r };

        public static ApiListResponse<ReservaAdminResponse> ToListResponse(DataPagedResult<ReservaAdminResponse> paged)
            => new()
            {
                Status = 200,
                Message = "Consulta exitosa",
                Data = paged.Items.ToList(),
                Pagination = new PaginationResponse
                {
                    Page = paged.Page,
                    Limit = paged.Limit,
                    Total = paged.TotalFiltrado,
                    TotalPages = paged.TotalPaginas
                }
            };
    }
}
