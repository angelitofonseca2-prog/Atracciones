using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Atracciones
{
    public class OpcionFiltroResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Tagname { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public ImagenFiltroResponse? Image { get; set; }
        public IList<OpcionFiltroResponse>? ChildFilterOptions { get; set; }
    }

    public class ImagenFiltroResponse
    {
        public string Url { get; set; } = string.Empty;
    }

    public class FiltrosAtraccionResponse
    {
        public IList<OpcionFiltroResponse> DestinationFilters { get; set; } = new List<OpcionFiltroResponse>();
        public IList<OpcionFiltroResponse> TypeFilters { get; set; } = new List<OpcionFiltroResponse>();
        public IList<OpcionFiltroResponse> LabelFilters { get; set; } = new List<OpcionFiltroResponse>();
        public IList<OpcionFiltroResponse> MinRatingFilter { get; set; } = new List<OpcionFiltroResponse>();
        public IList<OpcionFiltroResponse> TimeOfDayFilters { get; set; } = new List<OpcionFiltroResponse>();
        public IList<OpcionFiltroResponse> SupportedLanguageFilters { get; set; } = new List<OpcionFiltroResponse>();
        public IList<OpcionFiltroResponse> UfiFilters { get; set; } = new List<OpcionFiltroResponse>();
    }
}
