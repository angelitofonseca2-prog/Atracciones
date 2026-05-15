using Atracciones.MsAtracciones.DataAccess.Context;
using Atracciones.MsAtracciones.DataAccess.Entities;
using Atracciones.MsAtracciones.DataManagement.Interfaces;
using Atracciones.MsAtracciones.DataManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsAtracciones.DataAccess.Repositories;

public sealed class InventarioRepository : IInventarioRepository
{
    private readonly InventarioDbContext _db;

    public InventarioRepository(InventarioDbContext db) => _db = db;

    public async Task<PagedResult<AtraccionIndexRow>> ListarConFiltrosAsync(AtraccionFiltroQuery filtro, CancellationToken ct = default)
    {
        var query = _db.Atracciones.AsNoTracking()
            .Include(a => a.Categorias)
            .Include(a => a.Idiomas)
            .Include(a => a.Incluyes)
            .Include(a => a.Imagenes)
            .Include(a => a.Tickets).ThenInclude(t => t.Horarios)
            .Include(a => a.Resenias)
            .Where(a => a.AtEstado == 'A');

        if (!string.IsNullOrWhiteSpace(filtro.Ciudad))
            query = query.Where(a => EF.Functions.ILike(a.DesNombreSnap, filtro.Ciudad.Trim()));

        if (filtro.TipoCatGuid.HasValue)
        {
            var g = filtro.TipoCatGuid.Value;
            query = query.Where(a => a.Categorias.Any(c => c.CaEstado == 'A' && c.CatGuid == g));
        }

        if (filtro.SubtipoCatGuid.HasValue)
        {
            var g = filtro.SubtipoCatGuid.Value;
            query = query.Where(a => a.Categorias.Any(c => c.CaEstado == 'A' && c.CatGuid == g));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Etiqueta))
            query = query.Where(a => a.Incluyes.Any(i =>
                i.AiEstado == 'A' && i.IncDescripcionSnap == filtro.Etiqueta.Trim()));

        if (!string.IsNullOrWhiteSpace(filtro.Idioma))
            query = query.Where(a => a.Idiomas.Any(i =>
                i.IaEstado == 'A' && i.IdDescripcionSnap == filtro.Idioma.Trim()));

        if (filtro.CalificacionMin.HasValue)
        {
            var min = (double)filtro.CalificacionMin.Value;
            query = query.Where(a =>
                a.Resenias.Where(r => r.RsnEstado == 'A').Any() &&
                a.Resenias.Where(r => r.RsnEstado == 'A').Average(r => (double)r.RsnRating) >= min);
        }

        if (filtro.Disponible == true)
        {
            query = query.Where(a => a.AtDisponible && a.Tickets.Any(t =>
                t.TckEstado == 'A' &&
                t.Horarios.Any(h => h.HorEstado == 'A' && h.HorCuposDisponibles > 0)));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Horario))
        {
            var partes = filtro.Horario.Split('-');
            if (partes.Length == 2 &&
                TimeOnly.TryParse(partes[0], out var desde) &&
                TimeOnly.TryParse(partes[1], out var hasta))
            {
                query = query.Where(a => a.Tickets.Any(t =>
                    t.TckEstado == 'A' &&
                    t.Horarios.Any(h => h.HorEstado == 'A' && h.HorHoraInicio >= desde && h.HorHoraInicio < hasta)));
            }
        }

        var totalSinFiltros = await _db.Atracciones.AsNoTracking().CountAsync(a => a.AtEstado == 'A', ct);
        var totalFiltrado = await query.CountAsync(ct);

        query = filtro.OrdenarPor switch
        {
            "lowest_price" => query.OrderBy(a =>
                a.Tickets.Where(t => t.TckEstado == 'A').Min(t => (decimal?)t.TckPrecio) ?? decimal.MaxValue),
            "highest_weighted_rating" => query.OrderByDescending(a =>
                a.Resenias.Where(r => r.RsnEstado == 'A').Average(r => (double?)r.RsnRating) ?? 0),
            _ => query.OrderByDescending(a => a.AtTotalResenias),
        };

        var page = filtro.Page < 1 ? 1 : filtro.Page;
        var limit = filtro.Limit < 1 ? 10 : filtro.Limit;
        var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync(ct);

        var rows = items.Select(MapIndexRow).ToList();
        return new PagedResult<AtraccionIndexRow>(rows, totalFiltrado, totalSinFiltros);
    }

    private static AtraccionIndexRow MapIndexRow(AtraccionEntity a)
    {
        var ticketsActivos = a.Tickets.Where(t => t.TckEstado == 'A').ToList();
        var precioMin = ticketsActivos.Count > 0 ? ticketsActivos.Min(t => t.TckPrecio) : 0m;
        var calificacion = a.Resenias.Where(r => r.RsnEstado == 'A').Select(r => (double)r.RsnRating).ToList();
        double? avg = calificacion.Count > 0 ? calificacion.Average() : null;
        var tieneCupos = ticketsActivos.Any(t =>
            t.Horarios.Any(h => h.HorEstado == 'A' && h.HorCuposDisponibles > 0 && h.HorFecha >= DateOnly.FromDateTime(DateTime.UtcNow)));

        var catGuids = a.Categorias.Where(c => c.CaEstado == 'A').Select(c => c.CatGuid).ToList();
        var idiSnaps = a.Idiomas.Where(i => i.IaEstado == 'A').Select(i => i.IdDescripcionSnap).ToList();
        var incSnaps = a.Incluyes.Where(i => i.AiEstado == 'A').Select(i => i.IncDescripcionSnap).ToList();
        var img = a.Imagenes.Where(i => i.ImaEstado == 'A').OrderBy(i => i.ImaOrden).FirstOrDefault()?.ImgUrlSnap;

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var horariosFuturos = ticketsActivos.SelectMany(t => t.Horarios)
            .Where(h => h.HorEstado == 'A' && h.HorCuposDisponibles > 0 && h.HorFecha >= hoy)
            .OrderBy(h => h.HorFecha).ThenBy(h => h.HorHoraInicio)
            .ToList();
        var hayHoy = horariosFuturos.Any(h => h.HorFecha == hoy);
        var primero = horariosFuturos.FirstOrDefault();
        var proxFecha = primero?.HorFecha;
        var cupos = primero?.HorCuposDisponibles;

        return new AtraccionIndexRow(
            a.AtGuid,
            a.AtNombre,
            a.AtDescripcion,
            a.AtTotalResenias,
            a.AtDuracionMinutos,
            a.AtPrecioReferencia,
            a.AtDisponible,
            a.DesNombreSnap,
            a.DesPaisSnap,
            precioMin,
            avg,
            tieneCupos,
            catGuids,
            idiSnaps,
            incSnaps,
            img,
            hayHoy,
            proxFecha == hoy ? null : proxFecha,
            cupos);
    }

    public async Task<AtraccionDetalleRow?> ObtenerDetalleAsync(Guid atGuid, CancellationToken ct = default)
    {
        var a = await _db.Atracciones.AsNoTracking()
            .Include(x => x.Categorias)
            .Include(x => x.Idiomas)
            .Include(x => x.Imagenes)
            .Include(x => x.Incluyes)
            .Include(x => x.Resenias)
            .Include(x => x.Tickets)
            .Include(x => x.Tickets).ThenInclude(t => t.Horarios)
            .FirstOrDefaultAsync(x => x.AtGuid == atGuid && x.AtEstado == 'A', ct);
        if (a is null) return null;

        var categorias = a.Categorias.Where(c => c.CaEstado == 'A').Select(c => new AtraccionCategoriaRow(c.CatGuid, null)).ToList();
        var idiomas = a.Idiomas.Where(i => i.IaEstado == 'A').Select(i => new AtraccionIdiomaRow(i.IdGuid, i.IdDescripcionSnap)).ToList();
        var imgs = a.Imagenes.Where(i => i.ImaEstado == 'A').OrderBy(i => i.ImaOrden)
            .Select(i => new AtraccionImagenRow(i.ImgGuid, i.ImgUrlSnap, i.ImaOrden)).ToList();
        var incl = a.Incluyes.Where(i => i.AiEstado == 'A').Select(i => new AtraccionIncluyeRow(i.IncGuid, i.IncDescripcionSnap)).ToList();
        var tickets = a.Tickets.Where(t => t.TckEstado == 'A').Select(t => new TicketRow(t.TckGuid, t.TckTitulo, t.TckPrecio, t.TckTipoParticipante, t.TckCapacidadMaxima, t.TckCuposDisponibles)).ToList();
        var horarios = a.Tickets.Where(t => t.TckEstado == 'A').SelectMany(t => t.Horarios)
            .Where(h => h.HorEstado == 'A')
            .Select(h => new HorarioRow(h.HorGuid, h.TckGuid, h.HorFecha, h.HorHoraInicio, h.HorHoraFin, h.HorCuposDisponibles)).ToList();

        var calVals = a.Resenias.Where(r => r.RsnEstado == 'A').Select(r => (double)r.RsnRating).ToList();
        double? calAvg = calVals.Count > 0 ? calVals.Average() : null;

        return new AtraccionDetalleRow(
            a.AtGuid, a.AtNombre, a.AtDescripcion, a.DesNombreSnap, a.DesPaisSnap, a.AtTotalResenias, calAvg,
            a.AtDireccion, a.AtDuracionMinutos, a.AtPuntoEncuentro, a.AtPrecioReferencia,
            a.AtIncluyeAcompaniante, a.AtIncluyeTransporte, a.AtDisponible,
            categorias, idiomas, imgs, incl, tickets, horarios);
    }

    public async Task<IReadOnlyList<TicketRow>> ListarTicketsPorAtraccionAsync(Guid atGuid, CancellationToken ct = default)
    {
        var list = await _db.Tickets.AsNoTracking()
            .Where(t => t.AtGuid == atGuid && t.TckEstado == 'A')
            .Select(t => new TicketRow(t.TckGuid, t.TckTitulo, t.TckPrecio, t.TckTipoParticipante, t.TckCapacidadMaxima, t.TckCuposDisponibles))
            .ToListAsync(ct);
        return list;
    }

    public async Task<IReadOnlyList<HorarioRow>> ListarHorariosDisponiblesPorAtraccionAsync(Guid atGuid, CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _db.Horarios.AsNoTracking()
            .Where(h => h.HorEstado == 'A' && h.HorCuposDisponibles > 0 && h.HorFecha >= hoy &&
                        h.Ticket.AtGuid == atGuid && h.Ticket.TckEstado == 'A')
            .OrderBy(h => h.HorFecha).ThenBy(h => h.HorHoraInicio)
            .Select(h => new HorarioRow(h.HorGuid, h.TckGuid, h.HorFecha, h.HorHoraInicio, h.HorHoraFin, h.HorCuposDisponibles))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HorarioProximoRow>> ListarHorariosPorTicketGuidAsync(Guid tckGuid, CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = hoy.AddDays(7);
        return await _db.Horarios.AsNoTracking()
            .Where(h => h.TckGuid == tckGuid && h.HorEstado == 'A' && h.HorCuposDisponibles > 0 &&
                        h.HorFecha >= hoy && h.HorFecha <= limite)
            .OrderBy(h => h.HorFecha).ThenBy(h => h.HorHoraInicio)
            .Select(h => new HorarioProximoRow(h.HorGuid, h.TckGuid, h.HorFecha, h.HorHoraInicio, h.HorHoraFin, h.HorCuposDisponibles, h.Ticket.TckTitulo))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HorarioProximoRow>> ListarHorariosPorAtraccionVentanaAsync(Guid atGuid, int diasAdelante, CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = hoy.AddDays(Math.Max(1, diasAdelante));
        return await _db.Horarios.AsNoTracking()
            .Where(h => h.HorEstado == 'A' && h.HorCuposDisponibles > 0 && h.HorFecha >= hoy && h.HorFecha <= limite &&
                        h.Ticket.AtGuid == atGuid && h.Ticket.TckEstado == 'A')
            .OrderBy(h => h.HorFecha).ThenBy(h => h.HorHoraInicio)
            .Select(h => new HorarioProximoRow(h.HorGuid, h.TckGuid, h.HorFecha, h.HorHoraInicio, h.HorHoraFin, h.HorCuposDisponibles, h.Ticket.TckTitulo))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<AtraccionFiltroComputationRow>> ListarActivasParaFiltrosAsync(int maxItems, CancellationToken ct = default)
    {
        var take = maxItems < 1 ? 1000 : maxItems;
        var items = await _db.Atracciones.AsNoTracking()
            .Include(a => a.Categorias)
            .Include(a => a.Idiomas)
            .Include(a => a.Incluyes)
            .Include(a => a.Resenias)
            .Where(a => a.AtEstado == 'A')
            .OrderByDescending(a => a.AtTotalResenias)
            .Take(take)
            .ToListAsync(ct);

        return items.Select(a =>
        {
            var cats = a.Categorias.Where(c => c.CaEstado == 'A').Select(c => c.CatGuid).ToList();
            var idms = a.Idiomas.Where(i => i.IaEstado == 'A').ToList();
            var idiGuids = idms.Select(i => i.IdGuid).ToList();
            var idiSnaps = idms.Select(i => i.IdDescripcionSnap).ToList();
            var incSnaps = a.Incluyes.Where(i => i.AiEstado == 'A').Select(i => i.IncDescripcionSnap).ToList();
            var avg = a.Resenias.Where(r => r.RsnEstado == 'A').Select(r => (double)r.RsnRating).ToList();
            double? cal = avg.Count > 0 ? avg.Average() : null;
            return new AtraccionFiltroComputationRow(a.AtGuid, a.DesGuid, a.DesNombreSnap, cats, idiGuids, idiSnaps, incSnaps, cal);
        }).ToList();
    }

    public async Task<AtraccionAdminRow?> ObtenerAtraccionAdminAsync(Guid atGuid, CancellationToken ct = default)
    {
        var a = await _db.Atracciones.AsNoTracking().FirstOrDefaultAsync(x => x.AtGuid == atGuid, ct);
        return a is null ? null : MapAdmin(a);
    }

    public async Task<AtraccionAdminCompletaRow?> ObtenerAtraccionAdminCompletaAsync(Guid atGuid, CancellationToken ct = default)
    {
        var a = await _db.Atracciones.AsNoTracking()
            .Include(x => x.Categorias)
            .Include(x => x.Idiomas)
            .Include(x => x.Imagenes)
            .Include(x => x.Incluyes)
            .FirstOrDefaultAsync(x => x.AtGuid == atGuid && x.AtEstado == 'A', ct);
        if (a is null) return null;

        var baseRow = MapAdmin(a);
        var cats = a.Categorias.Where(c => c.CaEstado == 'A').Select(c => c.CatGuid).ToList();
        var idi = a.Idiomas.Where(i => i.IaEstado == 'A').ToList();
        var imgs = a.Imagenes.Where(i => i.ImaEstado == 'A').OrderBy(i => i.ImaOrden).ToList();
        var inc = a.Incluyes.Where(i => i.AiEstado == 'A').ToList();

        return new AtraccionAdminCompletaRow(
            baseRow,
            cats,
            idi.Select(i => i.IdGuid).ToList(),
            imgs.Select(i => i.ImgGuid).ToList(),
            inc.Select(i => i.IncGuid).ToList(),
            idi.Select(i => i.IdDescripcionSnap).ToList(),
            imgs.Select(i => i.ImgUrlSnap).ToList(),
            inc.Select(i => i.IncDescripcionSnap).ToList());
    }

    public async Task<PagedResult<AtraccionAdminRow>> ListarAtraccionesAdminAsync(AtraccionAdminFiltroQuery filtro, CancellationToken ct = default)
    {
        var q = _db.Atracciones.AsNoTracking().Where(a => a.AtEstado == 'A');
        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var b = filtro.Busqueda.Trim();
            q = q.Where(a => EF.Functions.ILike(a.AtNombre, "%" + b + "%"));
        }

        var total = await q.CountAsync(ct);
        var page = filtro.Page < 1 ? 1 : filtro.Page;
        var limit = filtro.Limit < 1 ? 10 : filtro.Limit;
        var entities = await q.OrderBy(a => a.AtNombre).Skip((page - 1) * limit).Take(limit).ToListAsync(ct);
        var items = entities.Select(MapAdmin).ToList();
        return new PagedResult<AtraccionAdminRow>(items, total, total);
    }

    private static AtraccionAdminRow MapAdmin(AtraccionEntity a) =>
        new(a.AtGuid, a.DesGuid, a.DesNombreSnap, a.DesPaisSnap, a.AtNumEstablecimiento, a.AtNombre, a.AtDescripcion, a.AtTotalResenias,
            a.AtDireccion, a.AtDuracionMinutos, a.AtPuntoEncuentro, a.AtPrecioReferencia, a.AtDisponible, a.AtEstado, a.AtFechaIngreso);

    public async Task<Guid> CrearAtraccionConRelacionesAsync(AtraccionPersistModel m, CancellationToken ct = default)
    {
        var g = m.AtGuid ?? Guid.NewGuid();
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var entity = new AtraccionEntity
            {
                AtGuid = g,
                DesGuid = m.DesGuid,
                DesNombreSnap = m.DesNombreSnap,
                DesPaisSnap = m.DesPaisSnap,
                AtNumEstablecimiento = m.NumEstablecimiento,
                AtNombre = m.Nombre,
                AtDescripcion = m.Descripcion,
                AtTotalResenias = 0,
                AtDireccion = m.Direccion,
                AtDuracionMinutos = m.DuracionMinutos,
                AtPuntoEncuentro = m.PuntoEncuentro,
                AtPrecioReferencia = m.PrecioReferencia,
                AtDisponible = m.Disponible,
                AtIncluyeAcompaniante = false,
                AtIncluyeTransporte = false,
                AtEstado = 'A',
                AtFechaIngreso = DateTime.UtcNow,
                AtUsuarioIngreso = m.Usuario,
                AtIpIngreso = m.Ip,
            };
            _db.Atracciones.Add(entity);
            await ReplaceRelacionesAsync(g, m, ct);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return g;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ActualizarAtraccionConRelacionesAsync(AtraccionPersistModel m, CancellationToken ct = default)
    {
        if (!m.AtGuid.HasValue) throw new InvalidOperationException("AtGuid requerido para actualizar.");
        var g = m.AtGuid.Value;
        var entity = await _db.Atracciones.FirstOrDefaultAsync(a => a.AtGuid == g && a.AtEstado == 'A', ct)
            ?? throw new InvalidOperationException("Atracción no encontrada.");
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            entity.DesGuid = m.DesGuid;
            entity.DesNombreSnap = m.DesNombreSnap;
            entity.DesPaisSnap = m.DesPaisSnap;
            entity.AtNumEstablecimiento = m.NumEstablecimiento;
            entity.AtNombre = m.Nombre;
            entity.AtDescripcion = m.Descripcion;
            entity.AtDireccion = m.Direccion;
            entity.AtDuracionMinutos = m.DuracionMinutos;
            entity.AtPuntoEncuentro = m.PuntoEncuentro;
            entity.AtPrecioReferencia = m.PrecioReferencia;
            entity.AtDisponible = m.Disponible;
            entity.AtFechaMod = DateTime.UtcNow;
            entity.AtUsuarioMod = m.Usuario;
            entity.AtIpMod = m.Ip;

            await ReplaceRelacionesAsync(g, m, ct);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private async Task ReplaceRelacionesAsync(Guid atGuid, AtraccionPersistModel m, CancellationToken ct)
    {
        await _db.AtraccionCategorias.Where(x => x.AtGuid == atGuid).ExecuteDeleteAsync(ct);
        await _db.AtraccionIdiomas.Where(x => x.AtGuid == atGuid).ExecuteDeleteAsync(ct);
        await _db.AtraccionImagenes.Where(x => x.AtGuid == atGuid).ExecuteDeleteAsync(ct);
        await _db.AtraccionIncluyes.Where(x => x.AtGuid == atGuid).ExecuteDeleteAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var cat in m.CategoriaGuids.Where(x => x != Guid.Empty).Distinct())
            _db.AtraccionCategorias.Add(new AtraccionCategoriaEntity { AtGuid = atGuid, CatGuid = cat, CaEstado = 'A', CaFechaIngreso = now, CaUsuarioIngreso = m.Usuario });

        foreach (var idioma in m.IdiomaGuids.Where(x => x != Guid.Empty).Distinct())
        {
            var desc = m.IdiomaDescripciones.GetValueOrDefault(idioma, string.Empty);
            _db.AtraccionIdiomas.Add(new AtraccionIdiomaEntity { AtGuid = atGuid, IdGuid = idioma, IdDescripcionSnap = desc, IaEstado = 'A', IaFechaIngreso = now, IaUsuarioIngreso = m.Usuario });
        }

        var orden = 0;
        foreach (var img in m.ImagenGuids.Where(x => x != Guid.Empty).Distinct())
        {
            var url = m.ImagenUrls.GetValueOrDefault(img, string.Empty);
            _db.AtraccionImagenes.Add(new AtraccionImagenEntity { AtGuid = atGuid, ImgGuid = img, ImgUrlSnap = url, ImaOrden = orden++, ImaEstado = 'A', ImaFechaIngreso = now, ImaUsuarioIngreso = m.Usuario });
        }

        foreach (var inc in m.IncluyeGuids.Where(x => x != Guid.Empty).Distinct())
        {
            var desc = m.IncluyeDescripciones.GetValueOrDefault(inc, string.Empty);
            _db.AtraccionIncluyes.Add(new AtraccionIncluyeEntity { AtGuid = atGuid, IncGuid = inc, IncDescripcionSnap = desc, AiEstado = 'A', AiFechaIngreso = now, AiUsuarioIngreso = m.Usuario });
        }
    }

    public async Task EliminarAtraccionLogicoAsync(Guid atGuid, string usuario, string ip, CancellationToken ct = default)
    {
        await _db.Atracciones.Where(a => a.AtGuid == atGuid && a.AtEstado == 'A')
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.AtEstado, _ => 'I')
                .SetProperty(a => a.AtFechaEliminacion, _ => DateTime.UtcNow)
                .SetProperty(a => a.AtUsuarioEliminacion, _ => usuario)
                .SetProperty(a => a.AtIpEliminacion, _ => ip), ct);
    }

    public Task<TicketAdminRow?> ObtenerTicketAdminAsync(Guid tckGuid, CancellationToken ct = default)
        => _db.Tickets.AsNoTracking()
            .Where(t => t.TckGuid == tckGuid)
            .Select(t => new TicketAdminRow(t.TckGuid, t.AtGuid, t.Atraccion.AtNombre, t.TckTitulo, t.TckPrecio, t.TckTipoParticipante, t.TckCapacidadMaxima, t.TckCuposDisponibles, t.TckEstado, t.TckFechaIngreso))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<TicketAdminRow>> ListarTicketsAdminAsync(CancellationToken ct = default)
        => await _db.Tickets.AsNoTracking().Where(t => t.TckEstado == 'A')
            .OrderBy(t => t.TckTitulo)
            .Select(t => new TicketAdminRow(t.TckGuid, t.AtGuid, t.Atraccion.AtNombre, t.TckTitulo, t.TckPrecio, t.TckTipoParticipante, t.TckCapacidadMaxima, t.TckCuposDisponibles, t.TckEstado, t.TckFechaIngreso))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TicketAdminRow>> ListarTicketsPorAtraccionAdminAsync(Guid atGuid, CancellationToken ct = default)
        => await _db.Tickets.AsNoTracking().Where(t => t.AtGuid == atGuid && t.TckEstado == 'A')
            .Select(t => new TicketAdminRow(t.TckGuid, t.AtGuid, t.Atraccion.AtNombre, t.TckTitulo, t.TckPrecio, t.TckTipoParticipante, t.TckCapacidadMaxima, t.TckCuposDisponibles, t.TckEstado, t.TckFechaIngreso))
            .ToListAsync(ct);

    public async Task<Guid> CrearTicketAsync(TicketPersistModel m, CancellationToken ct = default)
    {
        var g = m.TckGuid ?? Guid.NewGuid();
        _db.Tickets.Add(new TicketEntity
        {
            TckGuid = g,
            AtGuid = m.AtGuid,
            TckTitulo = m.Titulo,
            TckPrecio = m.Precio,
            TckTipoParticipante = m.TipoParticipante,
            TckCapacidadMaxima = m.CapacidadMaxima,
            TckCuposDisponibles = m.CuposDisponibles,
            TckEstado = 'A',
            TckFechaIngreso = DateTime.UtcNow,
            TckUsuarioIngreso = m.Usuario,
            TckIpIngreso = m.Ip,
        });
        await _db.SaveChangesAsync(ct);
        return g;
    }

    public async Task ActualizarTicketAsync(TicketPersistModel m, CancellationToken ct = default)
    {
        if (!m.TckGuid.HasValue) throw new InvalidOperationException("TckGuid requerido.");
        var t = await _db.Tickets.FirstOrDefaultAsync(x => x.TckGuid == m.TckGuid && x.TckEstado == 'A', ct)
            ?? throw new InvalidOperationException("Ticket no encontrado.");
        t.TckTitulo = m.Titulo;
        t.TckPrecio = m.Precio;
        t.TckTipoParticipante = m.TipoParticipante;
        t.TckCapacidadMaxima = m.CapacidadMaxima;
        t.TckCuposDisponibles = m.CuposDisponibles;
        t.TckFechaMod = DateTime.UtcNow;
        t.TckUsuarioMod = m.Usuario;
        t.TckIpMod = m.Ip;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EliminarTicketLogicoAsync(Guid tckGuid, string usuario, string ip, CancellationToken ct = default)
    {
        await _db.Tickets.Where(t => t.TckGuid == tckGuid && t.TckEstado == 'A')
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.TckEstado, _ => 'I')
                .SetProperty(t => t.TckFechaEliminacion, _ => DateTime.UtcNow)
                .SetProperty(t => t.TckUsuarioEliminacion, _ => usuario)
                .SetProperty(t => t.TckIpEliminacion, _ => ip), ct);
    }

    public Task<HorarioAdminRow?> ObtenerHorarioAdminAsync(Guid horGuid, CancellationToken ct = default)
        => _db.Horarios.AsNoTracking()
            .Where(h => h.HorGuid == horGuid)
            .Select(h => new HorarioAdminRow(h.HorGuid, h.TckGuid, h.Ticket.AtGuid, h.Ticket.Atraccion.AtNombre, h.Ticket.TckTitulo, h.Ticket.TckCapacidadMaxima, h.HorFecha, h.HorHoraInicio, h.HorHoraFin, h.HorCuposDisponibles, h.HorEstado, h.HorFechaIngreso))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<HorarioAdminRow>> ListarHorariosAdminAsync(CancellationToken ct = default)
        => await _db.Horarios.AsNoTracking().Where(h => h.HorEstado == 'A')
            .OrderBy(h => h.HorFecha).ThenBy(h => h.HorHoraInicio)
            .Select(h => new HorarioAdminRow(h.HorGuid, h.TckGuid, h.Ticket.AtGuid, h.Ticket.Atraccion.AtNombre, h.Ticket.TckTitulo, h.Ticket.TckCapacidadMaxima, h.HorFecha, h.HorHoraInicio, h.HorHoraFin, h.HorCuposDisponibles, h.HorEstado, h.HorFechaIngreso))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<HorarioAdminRow>> ListarHorariosPorTicketAdminAsync(Guid tckGuid, CancellationToken ct = default)
        => await _db.Horarios.AsNoTracking().Where(h => h.TckGuid == tckGuid && h.HorEstado == 'A')
            .Select(h => new HorarioAdminRow(h.HorGuid, h.TckGuid, h.Ticket.AtGuid, h.Ticket.Atraccion.AtNombre, h.Ticket.TckTitulo, h.Ticket.TckCapacidadMaxima, h.HorFecha, h.HorHoraInicio, h.HorHoraFin, h.HorCuposDisponibles, h.HorEstado, h.HorFechaIngreso))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<HorarioAdminRow>> ListarHorariosPorAtraccionAdminAsync(Guid atGuid, CancellationToken ct = default)
        => await _db.Horarios.AsNoTracking()
            .Where(h => h.HorEstado == 'A' && h.Ticket.AtGuid == atGuid && h.Ticket.TckEstado == 'A')
            .Select(h => new HorarioAdminRow(h.HorGuid, h.TckGuid, h.Ticket.AtGuid, h.Ticket.Atraccion.AtNombre, h.Ticket.TckTitulo, h.Ticket.TckCapacidadMaxima, h.HorFecha, h.HorHoraInicio, h.HorHoraFin, h.HorCuposDisponibles, h.HorEstado, h.HorFechaIngreso))
            .ToListAsync(ct);

    public async Task<Guid> CrearHorarioAsync(HorarioPersistModel m, CancellationToken ct = default)
    {
        var g = m.HorGuid ?? Guid.NewGuid();
        _db.Horarios.Add(new HorarioEntity
        {
            HorGuid = g,
            TckGuid = m.TckGuid,
            HorFecha = m.Fecha,
            HorHoraInicio = m.HoraInicio,
            HorHoraFin = m.HoraFin,
            HorCuposDisponibles = m.CuposDisponibles,
            HorEstado = 'A',
            HorFechaIngreso = DateTime.UtcNow,
            HorUsuarioIngreso = m.Usuario,
            HorIpIngreso = m.Ip,
        });
        await _db.SaveChangesAsync(ct);
        return g;
    }

    public async Task ActualizarHorarioAsync(HorarioPersistModel m, CancellationToken ct = default)
    {
        if (!m.HorGuid.HasValue) throw new InvalidOperationException("HorGuid requerido.");
        var h = await _db.Horarios.FirstOrDefaultAsync(x => x.HorGuid == m.HorGuid && x.HorEstado == 'A', ct)
            ?? throw new InvalidOperationException("Horario no encontrado.");
        h.HorFecha = m.Fecha;
        h.HorHoraInicio = m.HoraInicio;
        h.HorHoraFin = m.HoraFin;
        h.HorCuposDisponibles = m.CuposDisponibles;
        h.HorFechaMod = DateTime.UtcNow;
        h.HorUsuarioMod = m.Usuario;
        h.HorIpMod = m.Ip;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EliminarHorarioLogicoAsync(Guid horGuid, string usuario, string ip, CancellationToken ct = default)
    {
        await _db.Horarios.Where(h => h.HorGuid == horGuid && h.HorEstado == 'A')
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.HorEstado, _ => 'I')
                .SetProperty(h => h.HorFechaEliminacion, _ => DateTime.UtcNow)
                .SetProperty(h => h.HorUsuarioEliminacion, _ => usuario)
                .SetProperty(h => h.HorIpEliminacion, _ => ip), ct);
    }

    public async Task<(decimal Precio, string TipoParticipante, Guid AtGuid)?> ObtenerPrecioTicketActivoAsync(Guid tckGuid, CancellationToken ct = default)
    {
        var row = await _db.Tickets.AsNoTracking()
            .Where(t => t.TckGuid == tckGuid && t.TckEstado == 'A')
            .Select(t => new { t.TckPrecio, t.TckTipoParticipante, t.AtGuid })
            .FirstOrDefaultAsync(ct);
        return row is null ? null : (row.TckPrecio, row.TckTipoParticipante, row.AtGuid);
    }

    public async Task<(string AtNombre, DateOnly HorFecha, TimeOnly HorHoraInicio, TimeOnly? HorHoraFin)?> ObtenerHorarioReservaSnapshotAsync(
        Guid horGuid,
        Guid atGuidEsperado,
        CancellationToken ct = default)
    {
        var row = await _db.Horarios.AsNoTracking()
            .Include(h => h.Ticket)
            .ThenInclude(t => t.Atraccion)
            .Where(h => h.HorGuid == horGuid && h.HorEstado == 'A')
            .Select(h => new { h.HorFecha, h.HorHoraInicio, h.HorHoraFin, h.Ticket })
            .FirstOrDefaultAsync(ct);

        if (row?.Ticket is null || row.Ticket.TckEstado != 'A' || row.Ticket.AtGuid != atGuidEsperado)
            return null;

        var nombre = row.Ticket.Atraccion?.AtNombre;
        if (string.IsNullOrWhiteSpace(nombre) || row.Ticket.Atraccion!.AtEstado != 'A')
            return null;

        return (nombre, row.HorFecha, row.HorHoraInicio, row.HorHoraFin);
    }

    public async Task<int?> DescontarCuposHorarioAsync(Guid horGuid, int cantidad, CancellationToken ct = default)
    {
        if (cantidad <= 0) return null;
        var affected = await _db.Horarios
            .Where(h => h.HorGuid == horGuid && h.HorEstado == 'A' && h.HorCuposDisponibles >= cantidad)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.HorCuposDisponibles, h => h.HorCuposDisponibles - cantidad), ct);
        if (affected == 0) return null;
        return await _db.Horarios.AsNoTracking().Where(h => h.HorGuid == horGuid).Select(h => h.HorCuposDisponibles).FirstAsync(ct);
    }

    public async Task<int?> IncrementarCuposHorarioAsync(Guid horGuid, int cantidad, CancellationToken ct = default)
    {
        if (cantidad <= 0) return null;
        await _db.Horarios
            .Where(h => h.HorGuid == horGuid && h.HorEstado == 'A')
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.HorCuposDisponibles, h => h.HorCuposDisponibles + cantidad), ct);
        return await _db.Horarios.AsNoTracking().Where(h => h.HorGuid == horGuid).Select(h => h.HorCuposDisponibles).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<AtraccionFiltroSeedRow>> ListarSemillasFiltroAsync(int maxItems, CancellationToken ct = default)
    {
        var items = await _db.Atracciones.AsNoTracking()
            .Include(a => a.Categorias)
            .Include(a => a.Idiomas)
            .Include(a => a.Incluyes)
            .Include(a => a.Resenias)
            .Where(a => a.AtEstado == 'A')
            .OrderByDescending(a => a.AtTotalResenias)
            .Take(maxItems)
            .ToListAsync(ct);

        return items.Select(a =>
        {
            var cats = a.Categorias.Where(c => c.CaEstado == 'A').Select(c => c.CatGuid).ToList();
            var idiDesc = a.Idiomas.Where(i => i.IaEstado == 'A').Select(i => i.IdDescripcionSnap).ToList();
            var incDesc = a.Incluyes.Where(i => i.AiEstado == 'A').Select(i => i.IncDescripcionSnap).ToList();
            var avg = a.Resenias.Where(r => r.RsnEstado == 'A').Select(r => (double)r.RsnRating).ToList();
            double? cal = avg.Count > 0 ? avg.Average() : null;
            return new AtraccionFiltroSeedRow(a.AtGuid, a.DesGuid, a.DesNombreSnap, a.DesPaisSnap, cats, idiDesc, incDesc, cal);
        }).ToList();
    }

    public Task<bool> ExisteAtraccionActivaAsync(Guid atGuid, CancellationToken ct = default)
        => _db.Atracciones.AsNoTracking().AnyAsync(a => a.AtGuid == atGuid && a.AtEstado == 'A', ct);
}
