namespace Atracciones.MsAtracciones.Business.Common;

public sealed record DataPagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalFiltrado,
    int TotalSinFiltros,
    int Page,
    int Limit)
{
    public int TotalPaginas => Limit <= 0 ? 0 : (int)Math.Ceiling(TotalFiltrado / (double)Limit);
}
