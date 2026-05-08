using Microservicio.Atracciones.Api.Helpers;
using Microservicio.Atracciones.Api.Models.Common;
using Microservicio.Atracciones.Business.DTOs.Public.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Api.Mappers.Public
{
    public static class AtraccionesApiMapper
    {
        /// <summary>
        /// Construye la respuesta completa del listado paginado.
        /// Contrato sección 3.1 + 4.1.
        /// </summary>
        public static ApiListResponse<AtraccionListadoResponse> ToListadoResponse(
            DataPagedResult<AtraccionListadoResponse> paged,
            string baseUrl,
            string queryString,
            string ordenarPor)
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
                    TotalPages = paged.TotalPaginas
                },
                FilterStats = new FilterStatsResponse
                {
                    FilteredProductCount = paged.TotalFiltrado,
                    UnfilteredProductCount = paged.TotalSinFiltros
                },
                Sorters = SorterFactory.ObtenerSorters(),
                DefaultSorter = SorterFactory.ObtenerDefault(),
                Links = LinkBuilder.ParaListado(baseUrl, queryString, paged.Page, paged.Limit, paged.TotalPaginas)
            };
        }

        /// <summary>
        /// Construye la respuesta del detalle de una atracción.
        /// Contrato sección 3.3 + 4.2.
        /// </summary>
        public static ApiItemResponse<AtraccionDetalleResponse> ToDetalleResponse(
            AtraccionDetalleResponse detalle)
        {
            return new ApiItemResponse<AtraccionDetalleResponse>
            {
                Status = 200,
                Message = "Consulta exitosa",
                Data = detalle
            };
        }

        /// <summary>
        /// Construye la respuesta de filtros disponibles.
        /// Contrato sección 4.3.
        /// </summary>
        public static ApiItemResponse<FiltrosAtraccionResponse> ToFiltrosResponse(
            FiltrosAtraccionResponse filtros)
        {
            return new ApiItemResponse<FiltrosAtraccionResponse>
            {
                Status = 200,
                Message = "Filtros obtenidos exitosamente",
                Data = filtros
            };
        }
    }
}
