using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Admin.Clientes;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Api.Mappers.Admin
{
    public static class ClientesApiMapper
    {
        public static ApiItemResponse<ClienteResponse> ToResponse(ClienteResponse c, string msg = "Ok")
            => new() { Status = 200, Message = msg, Data = c };

        public static ApiListResponse<ClienteResponse> ToListResponse(DataPagedResult<ClienteResponse> paged)
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
