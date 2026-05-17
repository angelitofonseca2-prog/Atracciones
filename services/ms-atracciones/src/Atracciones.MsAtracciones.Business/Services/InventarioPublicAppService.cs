using System.Globalization;
using Atracciones.Contracts.Catalogos.V1;
using Atracciones.MsAtracciones.Business.Common;
using Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;
using Atracciones.MsAtracciones.Business.Exceptions;
using Atracciones.MsAtracciones.Business.Integration;
using Atracciones.MsAtracciones.Business.Mappers;
using Atracciones.MsAtracciones.Business.Validation;
using Atracciones.MsAtracciones.DataManagement.Interfaces;
using Atracciones.MsAtracciones.DataManagement.Models;

namespace Atracciones.MsAtracciones.Business.Services;

public sealed class InventarioPublicAppService : IInventarioPublicAppService
{
    private readonly IInventarioRepository _repo;
    private readonly ICatalogoGrpcClient _catalog;

    public InventarioPublicAppService(IInventarioRepository repo, ICatalogoGrpcClient catalog)
    {
        _repo = repo;
        _catalog = catalog;
    }

    public async Task<DataPagedResult<AtraccionListadoResponse>> ListarAsync(
        AtraccionFiltroRequest request,
        string baseUrl,
        CancellationToken ct = default)
    {
        AtraccionPublicValidator.Validar(request);

        var filtro = new AtraccionFiltroQuery(
            Ciudad: request.Ciudad,
            TipoCatGuid: Guid.TryParse(request.Tipo, out var tg) ? tg : null,
            SubtipoCatGuid: Guid.TryParse(request.Subtipo, out var sg) ? sg : null,
            Etiqueta: request.Etiqueta,
            Idioma: request.Idioma,
            CalificacionMin: request.CalificacionMin,
            Horario: request.Horario,
            Disponible: request.Disponible,
            OrdenarPor: request.OrdenarPor,
            Page: request.Page,
            Limit: request.Limit);

        var paged = await _repo.ListarConFiltrosAsync(filtro, ct);
        var catGuids = paged.Items.SelectMany(i => i.CatGuids).Distinct().ToList();
        var catMap = await _catalog.ObtenerCategoriasConAncestrosAsync(catGuids, ct);

        var responses = paged.Items.Select(row =>
        {
            var rel = row.CatGuids.Select(g => catMap.GetValueOrDefault(g)).Where(x => x != null).Cast<CategoriaGrpc>().ToList();
            return AtraccionPublicMapper.ToListadoResponse(row, rel, baseUrl);
        }).ToList();

        return new DataPagedResult<AtraccionListadoResponse>(responses, paged.TotalFiltrado, paged.TotalSinFiltros, request.Page, request.Limit);
    }

    public async Task<AtraccionDetalleResponse> ObtenerPorGuidAsync(Guid atGuid, string baseUrl, CancellationToken ct = default)
    {
        var model = await _repo.ObtenerDetalleAsync(atGuid, ct)
            ?? throw new NotFoundException("Atracción", atGuid);

        var catMap = await _catalog.ObtenerCategoriasConAncestrosAsync(model.Categorias.Select(c => c.CatGuid), ct);
        var rel = model.Categorias.Select(c => catMap.GetValueOrDefault(c.CatGuid)).Where(x => x != null).Cast<CategoriaGrpc>().ToList();

        var disp = ComputeDisp(model.Horarios);
        var horariosProximos = await _repo.ListarHorariosPorAtraccionVentanaAsync(atGuid, 7, ct);
        var horariosResp = horariosProximos.Select(ToHorarioProximoResponse).ToList();

        var ticketsRes = await BuildTicketsDisponiblesAsync(atGuid, ct);

        return AtraccionPublicMapper.ToDetalleResponse(model, rel, disp, ticketsRes, horariosResp, baseUrl, model.DesNombre);
    }

    public async Task<FiltrosAtraccionResponse> ObtenerFiltrosAsync(CancellationToken ct = default)
    {
        var atracciones = await _repo.ListarActivasParaFiltrosAsync(1000, ct);
        var total = atracciones.Count;

        var desGuids = atracciones.Select(a => a.DesGuid.ToString()).Distinct().ToList();
        var catGuids = atracciones.SelectMany(a => a.CatGuids).Distinct().ToList();
        var idiGuids = atracciones.SelectMany(a => a.IdiomaGuids).Distinct().ToList();

        var catMap = await _catalog.ObtenerCategoriasConAncestrosAsync(catGuids, ct);

        var req = new GetCatalogosPorGuidsRequest();
        foreach (var d in desGuids) req.DestinoGuids.Add(d);
        foreach (var i in idiGuids) req.IdiomaGuids.Add(i.ToString());

        var resp = await _catalog.GetCatalogosPorGuidsAsync(req, ct);

        var destinos = resp.Destinos.Where(EsActivo).ToList();
        var destinationFilters = destinos
            .Select(destino => new OpcionFiltroResponse
            {
                Name = destino.Nombre,
                Tagname = destino.DesGuid,
                ProductCount = atracciones.Count(a => a.DesGuid.ToString() == destino.DesGuid),
                Image = string.IsNullOrWhiteSpace(destino.ImagenUrl)
                    ? null
                    : new ImagenFiltroResponse { Url = destino.ImagenUrl },
            })
            .ToList();

        var todasCats = catMap.Values.Where(EsActivo).ToList();
        var categoriasRaiz = todasCats.Where(c => string.IsNullOrWhiteSpace(c.ParentGuid)).ToList();
        var typeFilters = new List<OpcionFiltroResponse>();
        foreach (var cat in categoriasRaiz)
        {
            var hijosCatalogo = todasCats.Where(h =>
                string.Equals(h.ParentGuid, cat.CatGuid, StringComparison.OrdinalIgnoreCase)).ToList();
            var catGuid = Guid.Parse(cat.CatGuid);
            var hijos = hijosCatalogo.Select(h => new OpcionFiltroResponse
            {
                Name = h.Nombre,
                Tagname = h.CatGuid,
                ProductCount = atracciones.Count(a => a.CatGuids.Contains(Guid.Parse(h.CatGuid))),
                Image = null,
            }).ToList();

            var hijoGuids = hijosCatalogo.Select(h => Guid.Parse(h.CatGuid)).ToHashSet();
            var productCount = atracciones.Count(a =>
                a.CatGuids.Contains(catGuid) || a.CatGuids.Any(g => hijoGuids.Contains(g)));

            typeFilters.Add(new OpcionFiltroResponse
            {
                Name = cat.Nombre,
                Tagname = cat.CatGuid,
                ProductCount = productCount,
                Image = null,
                ChildFilterOptions = hijos.Count > 0 ? hijos : null,
            });
        }

        var etiquetasConteo = atracciones
            .SelectMany(a => a.IncluyeSnaps)
            .Where(i => !i.StartsWith("NO:", StringComparison.Ordinal))
            .GroupBy(i => i)
            .Select(g => (Descripcion: g.Key, Conteo: g.Count()))
            .ToList();

        var idiomasGrpc = resp.Idiomas.Where(EsActivo).ToList();
        var idiomasConteo = idiomasGrpc
            .Select(i => (
                Descripcion: i.Descripcion,
                Guid: i.IdGuid,
                Conteo: atracciones.Count(a => a.IdiomaGuids.Contains(Guid.Parse(i.IdGuid)))))
            .ToList();

        var minRatingFilters = new[] { 4.5, 4.0, 3.5, 3.0 }
            .Select(r => new OpcionFiltroResponse
            {
                Name = $"{r:F1} y más",
                Tagname = $"{r:F1}",
                ProductCount = atracciones.Count(a =>
                    a.CalificacionPromedio.HasValue && a.CalificacionPromedio.Value >= r),
            }).ToList();

        var timeOfDayFilters = new List<OpcionFiltroResponse>
        {
            new() { Name = "Mañanas", Tagname = "05:00-12:00", ProductCount = 0 },
            new() { Name = "Tardes", Tagname = "12:00-18:00", ProductCount = 0 },
            new() { Name = "Noches", Tagname = "18:00-05:00", ProductCount = 0 },
        };

        return new FiltrosAtraccionResponse
        {
            DestinationFilters = destinationFilters,
            TypeFilters = typeFilters,
            LabelFilters = etiquetasConteo.Select(e => new OpcionFiltroResponse
            {
                Name = e.Descripcion,
                Tagname = e.Descripcion,
                ProductCount = e.Conteo,
            }).ToList(),
            MinRatingFilter = minRatingFilters,
            TimeOfDayFilters = timeOfDayFilters,
            SupportedLanguageFilters = idiomasConteo.Select(i => new OpcionFiltroResponse
            {
                Name = i.Descripcion,
                Tagname = i.Guid,
                ProductCount = i.Conteo,
            }).ToList(),
            UfiFilters =
            [
                new OpcionFiltroResponse { Name = "Todos", Tagname = "todos", ProductCount = total },
            ],
        };
    }

    public async Task<IReadOnlyList<TicketDisponibleResponse>> ListarTicketsAsync(Guid atGuid, CancellationToken ct = default)
    {
        _ = await _repo.ObtenerDetalleAsync(atGuid, ct)
            ?? throw new NotFoundException("Atracción", atGuid);

        return await BuildTicketsDisponiblesAsync(atGuid, ct);
    }

    public async Task<IReadOnlyList<HorarioProximoResponse>> ListarHorariosPorTicketAsync(Guid tckGuid, CancellationToken ct = default)
    {
        var rows = await _repo.ListarHorariosPorTicketGuidAsync(tckGuid, ct);
        if (rows.Count == 0)
        {
            _ = await _repo.ObtenerTicketAdminAsync(tckGuid, ct)
                ?? throw new NotFoundException("Ticket", tckGuid);
        }

        return rows.Select(ToHorarioProximoResponse).ToList();
    }

    public async Task<IReadOnlyList<HorarioProximoResponse>> ListarHorariosDisponiblesAsync(Guid atGuid, CancellationToken ct = default)
    {
        _ = await _repo.ObtenerDetalleAsync(atGuid, ct)
            ?? throw new NotFoundException("Atracción", atGuid);

        var rows = await _repo.ListarHorariosPorAtraccionVentanaAsync(atGuid, 365, ct);
        return rows.Where(EsHorarioDisponible).Select(ToHorarioProximoResponse).ToList();
    }

    private async Task<List<TicketDisponibleResponse>> BuildTicketsDisponiblesAsync(Guid atGuid, CancellationToken ct)
    {
        var tickets = await _repo.ListarTicketsPorAtraccionAsync(atGuid, ct);
        var horarios = await _repo.ListarHorariosPorAtraccionVentanaAsync(atGuid, 365, ct);
        var horariosDisp = horarios.Where(EsHorarioDisponible).ToList();

        return tickets.Select(t =>
        {
            var hs = horariosDisp.Where(h => h.TckGuid == t.TckGuid).Select(ToHorarioProximoResponse).ToList();
            return new TicketDisponibleResponse
            {
                TckGuid = t.TckGuid.ToString(),
                Titulo = t.TckTitulo,
                Tipo = t.TckTipoParticipante,
                Precio = t.TckPrecio,
                Moneda = "USD",
                CuposDisponibles = t.TckCuposDisponibles,
                HorariosDisponibles = hs,
            };
        }).ToList();
    }

    private static bool EsActivo(DestinoGrpc d)
        => string.Equals(d.Estado, "A", StringComparison.OrdinalIgnoreCase);

    private static bool EsActivo(CategoriaGrpc c)
        => string.Equals(c.Estado, "A", StringComparison.OrdinalIgnoreCase);

    private static bool EsActivo(IdiomaGrpc i)
        => string.Equals(i.Estado, "A", StringComparison.OrdinalIgnoreCase);

    private static bool EsHorarioDisponible(HorarioProximoRow h)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var fin = h.HorFechaFin ?? h.HorFecha;
        return h.HorCuposDisponibles > 0 && fin >= hoy;
    }

    private static HorarioProximoResponse ToHorarioProximoResponse(HorarioProximoRow h)
    {
        var fin = h.HorFechaFin ?? h.HorFecha;
        return new()
        {
            HorGuid = h.HorGuid.ToString(),
            TckGuid = h.TckGuid.ToString(),
            TicketTitulo = h.TicketTitulo,
            Fecha = h.HorFecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            FechaFin = fin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            HoraInicio = h.HorHoraInicio.ToString("HH:mm", CultureInfo.InvariantCulture),
            HoraFin = h.HorHoraFin?.ToString("HH:mm", CultureInfo.InvariantCulture),
            Cupos = h.HorCuposDisponibles,
            Disponible = EsHorarioDisponible(h),
        };
    }

    private static (bool DisponibleHoy, DateOnly? ProximaFecha, int? Cupos) ComputeDisp(IReadOnlyList<HorarioRow> horarios)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var activos = horarios
            .Where(h => h.HorCuposDisponibles > 0 && (h.HorFechaFin ?? h.HorFecha) >= hoy)
            .OrderBy(h => h.HorFecha).ThenBy(h => h.HorHoraInicio)
            .ToList();
        var hayHoy = activos.Any(h => h.HorFecha <= hoy && (h.HorFechaFin ?? h.HorFecha) >= hoy);
        var primero = activos.FirstOrDefault();
        DateOnly? prox = primero?.HorFecha;
        if (primero is not null && primero.HorFecha < hoy && (primero.HorFechaFin ?? primero.HorFecha) >= hoy)
            prox = hoy;
        var cupos = primero?.HorCuposDisponibles;
        return (hayHoy, prox == hoy ? null : prox, cupos);
    }
}
