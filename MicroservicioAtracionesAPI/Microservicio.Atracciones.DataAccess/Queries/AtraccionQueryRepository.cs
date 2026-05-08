using Microsoft.EntityFrameworkCore;
using Microservicio.Atracciones.DataAccess.Common;
using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microservicio.Atracciones.DataAccess.Entities.Catalogos;

namespace Microservicio.Atracciones.DataAccess.Queries;

public record AtraccionFiltroQuery(
    string? Ciudad = null,
    Guid? TipoCatGuid = null,
    Guid? SubtipoCatGuid = null,
    string? Etiqueta = null,
    string? Idioma = null,
    decimal? CalificacionMin = null,
    string? Horario = null,
    bool? Disponible = null,
    string OrdenarPor = "trending",
    int Page = 1,
    int Limit = 10
);

public record AtraccionListadoProjection(
    int AtId,
    Guid AtGuid,
    string AtNombre,
    string? AtDescripcion,
    int AtTotalResenias,
    int? AtDuracionMinutos,
    decimal? AtPrecioReferencia,
    bool AtDisponible,
    int DesId,
    string DesNombre,
    string DesPais
);

public class AtraccionQueryRepository
{
    private readonly AtraccionesDbContext _context;

    public AtraccionQueryRepository(AtraccionesDbContext context)
        => _context = context;

    public async Task<PagedResult<AtraccionEntity>> ListarConFiltrosAsync(AtraccionFiltroQuery filtro)
    {
        var query = _context.Atracciones
            .AsNoTracking()
            .Include(x => x.Destino)
            .Include(x => x.CategoriasAtracciones).ThenInclude(ca => ca.Categoria).ThenInclude(c => c.Parent)
            .Include(x => x.IdiomasAtracciones).ThenInclude(ia => ia.Idioma)
            .Include(x => x.ImagenesAtracciones).ThenInclude(ima => ima.Imagen)
            .Include(x => x.AtraccionesIncluyen).ThenInclude(ai => ai.Incluye)
            .Include(x => x.Tickets)
            .Include(x => x.Resenias)
            .Where(x => x.AtEstado == 'A');

        if (!string.IsNullOrWhiteSpace(filtro.Ciudad))
            query = query.Where(x => EF.Functions.ILike(x.Destino.DesNombre, filtro.Ciudad.Trim()));

        if (filtro.TipoCatGuid.HasValue)
            query = query.Where(x => x.CategoriasAtracciones.Any(ca =>
                ca.CaEstado == 'A' &&
                (ca.Categoria.CatGuid == filtro.TipoCatGuid.Value ||
                 (ca.Categoria.Parent != null && ca.Categoria.Parent.CatGuid == filtro.TipoCatGuid.Value))));

        if (filtro.SubtipoCatGuid.HasValue)
            query = query.Where(x => x.CategoriasAtracciones.Any(ca =>
                ca.CaEstado == 'A' && ca.Categoria.CatGuid == filtro.SubtipoCatGuid.Value));

        if (!string.IsNullOrWhiteSpace(filtro.Etiqueta))
            query = query.Where(x => x.AtraccionesIncluyen.Any(ai =>
                ai.AiEstado == 'A' && ai.Incluye.IncDescripcion == filtro.Etiqueta));

        if (!string.IsNullOrWhiteSpace(filtro.Idioma))
            query = query.Where(x => x.IdiomasAtracciones.Any(ia =>
                ia.IaEstado == 'A' && ia.Idioma.IdDescripcion == filtro.Idioma));

        if (filtro.CalificacionMin.HasValue)
            query = query.Where(x =>
                x.Resenias.Where(r => r.RsnEstado == 'A').Any() &&
                x.Resenias.Where(r => r.RsnEstado == 'A').Average(r => (double)r.RsnRating) >= (double)filtro.CalificacionMin.Value);

        if (filtro.Disponible == true)
            query = query.Where(x => x.AtDisponible && x.Tickets.Any(t =>
                t.TckEstado == 'A' &&
                t.Horarios.Any(h => h.HorEstado == 'A' && h.HorCuposDisponibles > 0)));

        if (!string.IsNullOrWhiteSpace(filtro.Horario))
        {
            var partes = filtro.Horario.Split('-');
            if (partes.Length == 2 &&
                TimeOnly.TryParse(partes[0], out var desde) &&
                TimeOnly.TryParse(partes[1], out var hasta))
            {
                query = query.Where(x => x.Tickets.Any(t =>
                    t.TckEstado == 'A' &&
                    t.Horarios.Any(h => h.HorEstado == 'A' && h.HorHoraInicio >= desde && h.HorHoraInicio < hasta)));
            }
        }

        // ── DIAGNÓSTICO TEMPORAL ELIMINADO ─────────────────────────────────────

        var totalSinFiltros = await _context.Atracciones
            .AsNoTracking()
            .CountAsync(x => x.AtEstado == 'A');

        var totalFiltrado = await query.CountAsync();

        query = filtro.OrdenarPor switch
        {
            "lowest_price" => query.OrderBy(x => x.Tickets.Where(t => t.TckEstado == 'A').Min(t => (decimal?)t.TckPrecio) ?? decimal.MaxValue),
            "highest_weighted_rating" => query.OrderByDescending(x => x.Resenias.Where(r => r.RsnEstado == 'A').Average(r => (double?)r.RsnRating) ?? 0),
            _ => query.OrderByDescending(x => x.AtTotalResenias),
        };

        var page = filtro.Page < 1 ? 1 : filtro.Page;
        var limit = filtro.Limit < 1 ? 10 : filtro.Limit;
        var skip = (page - 1) * limit;
        var items = await query.Skip(skip).Take(limit).ToListAsync();

        return new PagedResult<AtraccionEntity>(items, totalFiltrado, totalSinFiltros, filtro.Page, filtro.Limit);
    }

    public async Task<AtraccionEntity?> ObtenerDetallePublicoAsync(Guid atGuid)
        => await _context.Atracciones
            .AsNoTracking()
            .Include(x => x.Destino)
            .Include(x => x.CategoriasAtracciones).ThenInclude(ca => ca.Categoria).ThenInclude(c => c.Parent)
            .Include(x => x.IdiomasAtracciones).ThenInclude(ia => ia.Idioma)
            .Include(x => x.ImagenesAtracciones).ThenInclude(ima => ima.Imagen)
            .Include(x => x.AtraccionesIncluyen).ThenInclude(ai => ai.Incluye)
            .Include(x => x.Tickets.Where(t => t.TckEstado == 'A'))
            .Include(x => x.Resenias.Where(r => r.RsnEstado == 'A'))
            .FirstOrDefaultAsync(x => x.AtGuid == atGuid && x.AtEstado == 'A');

    public async Task<List<CategoriaEntity>> ObtenerCategoriasPorCiudadAsync(string? ciudad)
    {
        var query = _context.Categorias
            .AsNoTracking()
            .Where(c => c.CatEstado == 'A' &&
                        c.CategoriasAtracciones.Any(ca =>
                            ca.CaEstado == 'A' &&
                            ca.Atraccion.AtEstado == 'A'));

        if (!string.IsNullOrWhiteSpace(ciudad))
        {
            query = query.Where(c => c.CategoriasAtracciones.Any(ca =>
                ca.CaEstado == 'A' &&
                ca.Atraccion.AtEstado == 'A' &&
                EF.Functions.ILike(ca.Atraccion.Destino.DesNombre, ciudad.Trim())));
        }

        return await query
            .Include(c => c.Hijos.Where(h => h.CatEstado == 'A'))
            .Where(c => c.CatParentId == null)
            .ToListAsync();
    }

    public async Task<List<IdiomaEntity>> ObtenerIdiomasPorCiudadAsync(string ciudad)
        => await _context.Idiomas
            .AsNoTracking()
            .Where(i => i.IdEstado == 'A' &&
                        i.IdiomasAtracciones.Any(ia =>
                            ia.IaEstado == 'A' &&
                            ia.Atraccion.AtEstado == 'A' &&
                            EF.Functions.ILike(ia.Atraccion.Destino.DesNombre, ciudad.Trim())))
            .ToListAsync();

    public async Task<List<IncluyeEntity>> ObtenerEtiquetasPorCiudadAsync(string ciudad)
        => await _context.Incluyes
            .AsNoTracking()
            .Where(i => i.IncEstado == 'A' &&
                        i.AtraccionesIncluyen.Any(ai =>
                            ai.AiEstado == 'A' &&
                            ai.Atraccion.AtEstado == 'A' &&
                            EF.Functions.ILike(ai.Atraccion.Destino.DesNombre, ciudad.Trim())))
            .ToListAsync();

    public async Task<Dictionary<string, int>> ContarPorEtiquetaAsync(string ciudad)
        => await _context.Incluyes
            .AsNoTracking()
            .Where(i => i.IncEstado == 'A')
            .Select(i => new
            {
                i.IncDescripcion,
                Count = i.AtraccionesIncluyen.Count(ai =>
                    ai.AiEstado == 'A' &&
                    ai.Atraccion.AtEstado == 'A' &&
                    EF.Functions.ILike(ai.Atraccion.Destino.DesNombre, ciudad.Trim()))
            })
            .ToDictionaryAsync(x => x.IncDescripcion, x => x.Count);
}
