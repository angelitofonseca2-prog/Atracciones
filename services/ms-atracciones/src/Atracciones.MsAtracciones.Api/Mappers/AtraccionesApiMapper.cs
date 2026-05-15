using Atracciones.MsAtracciones.Api.Helpers;
using Atracciones.MsAtracciones.Api.Models.Common;
using Atracciones.MsAtracciones.Business.Common;
using Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

namespace Atracciones.MsAtracciones.Api.Mappers;

public static class AtraccionesApiMapper
{
    public static ApiListResponse<AtraccionListadoResponse> ToListadoResponse(
        DataPagedResult<AtraccionListadoResponse> paged,
        string baseUrl,
        string queryString)
    {
        return new ApiListResponse<AtraccionListadoResponse>
        {
            Status = 200,
            Message = paged.Items.Any()
                ? "Consulta exitosa"
                : "No se encontraron atracciones con los filtros aplicados",
            Data = paged.Items.ToList(),
            Pagination = new PaginationResponse
            {
                Page = paged.Page,
                Limit = paged.Limit,
                Total = paged.TotalFiltrado,
                TotalPages = paged.TotalPaginas,
            },
            FilterStats = new FilterStatsResponse
            {
                FilteredProductCount = paged.TotalFiltrado,
                UnfilteredProductCount = paged.TotalSinFiltros,
            },
            Sorters = SorterFactory.ObtenerSorters(),
            DefaultSorter = SorterFactory.ObtenerDefault(),
            Links = LinkBuilder.ParaListado(baseUrl, queryString, paged.Page, paged.Limit, paged.TotalPaginas),
        };
    }

    public static ApiItemResponse<AtraccionDetalleResponse> ToDetalleResponse(AtraccionDetalleResponse detalle) =>
        new()
        {
            Status = 200,
            Message = "Consulta exitosa",
            Data = detalle,
        };

    public static ApiItemResponse<FiltrosAtraccionResponse> ToFiltrosResponse(FiltrosAtraccionResponse filtros) =>
        new()
        {
            Status = 200,
            Message = "Filtros obtenidos exitosamente",
            Data = filtros,
        };
}
