using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Api.Mappers.Admin
{
    public static class AtraccionesAdminApiMapper
    {
        public static ApiItemResponse<AtraccionAdminResponse> ToResponse(AtraccionAdminResponse a, string msg = "Ok")
            => new() { Status = 200, Message = msg, Data = a };

        public static ApiListResponse<AtraccionAdminResponse> ToListResponse(DataPagedResult<AtraccionAdminResponse> paged)
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
