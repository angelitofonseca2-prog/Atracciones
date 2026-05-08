using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Common
{
    public class DataPagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalFiltrado { get; }
        public int TotalSinFiltros { get; }
        public int Page { get; }
        public int Limit { get; }
        public int TotalPaginas => Limit > 0 ? (int)Math.Ceiling(TotalFiltrado / (double)Limit) : 0;
        public bool TieneSiguiente => Page < TotalPaginas;
        public bool TieneAnterior => Page > 1;

        public DataPagedResult(IReadOnlyList<T> items, int totalFiltrado, int totalSinFiltros, int page, int limit)
        {
            Items = items;
            TotalFiltrado = totalFiltrado;
            TotalSinFiltros = totalSinFiltros;
            Page = page;
            Limit = limit;
        }
    }
}
