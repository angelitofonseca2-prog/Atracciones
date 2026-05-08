using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Facturas;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Api.Mappers.Admin
{
    public static class FacturasApiMapper
    {
        public static ApiItemResponse<FacturaResponse> ToResponse(FacturaResponse f, string msg = "Ok")
            => new() { Status = 200, Message = msg, Data = f };

        public static ApiListResponse<FacturaResponse> ToListResponse(DataPagedResult<FacturaResponse> paged)
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
