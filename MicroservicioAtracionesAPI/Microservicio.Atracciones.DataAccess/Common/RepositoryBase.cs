namespace Microservicio.Atracciones.DataAccess.Common;

public abstract class RepositoryBase
{
    protected static int CalcularSkip(int page, int limit)
    {
        if (page < 1) page = 1;
        if (limit < 1) limit = 10;
        return (page - 1) * limit;
    }

    protected static (int PageSegura, int LimitSeguro) NormalizarPaginacion(int page, int limit)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 50);
        return (page, limit);
    }
}