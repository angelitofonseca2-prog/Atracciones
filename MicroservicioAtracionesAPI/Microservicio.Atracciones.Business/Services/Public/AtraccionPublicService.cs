using Microservicio.Atracciones.Business.DTOs.Public.Atracciones;
using Microservicio.Atracciones.Business.Exceptions;
using Microservicio.Atracciones.Business.Interfaces.Public;
using Microservicio.Atracciones.Business.Mappers.Public;
using Microservicio.Atracciones.Business.Validators.Public;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
using System.Globalization;

namespace Microservicio.Atracciones.Business.Services.Public
{
    public class AtraccionPublicService : IAtraccionPublicService
    {
        private readonly IAtraccionDataService _atraccionService;
        private readonly ITicketDataService _ticketService;
        private readonly IDestinoDataService _destinoService;
        private readonly ICategoriaDataService _categoriaService;
        private readonly IIdiomaDataService _idiomaService;

        public AtraccionPublicService(
            IAtraccionDataService atraccionService,
            ITicketDataService ticketService,
            IDestinoDataService destinoService,
            ICategoriaDataService categoriaService,
            IIdiomaDataService idiomaService)
        {
            _atraccionService = atraccionService;
            _ticketService = ticketService;
            _destinoService = destinoService;
            _categoriaService = categoriaService;
            _idiomaService = idiomaService;
        }

        public async Task<DataPagedResult<AtraccionListadoResponse>> ListarAsync(
            AtraccionFiltroRequest request, string baseUrl)
        {
            AtraccionPublicValidator.Validar(request);

            var filtro = new AtraccionFiltroDataModel
            {
                Ciudad        = request.Ciudad,
                TipoCatGuid   = request.Tipo    is not null ? Guid.Parse(request.Tipo)    : null,
                SubtipoCatGuid= request.Subtipo is not null ? Guid.Parse(request.Subtipo) : null,
                Etiqueta      = request.Etiqueta,
                Idioma        = request.Idioma,
                CalificacionMin = request.CalificacionMin,
                Horario       = request.Horario,
                Disponible    = request.Disponible,
                OrdenarPor    = request.OrdenarPor,
                Page          = request.Page,
                Limit         = request.Limit
            };

            var paged = await _atraccionService.ListarConFiltrosAsync(filtro);

            // #4: batch query — una sola llamada en lugar de N queries independientes
            var atIds    = paged.Items.Select(a => a.AtId).ToList();
            var dispBatch = await _ticketService.ObtenerDisponibilidadBatchAsync(atIds);

            var responses = paged.Items
                .Select(model =>
                {
                    dispBatch.TryGetValue(model.AtId, out var disp);
                    return AtraccionPublicMapper.ToListadoResponse(model, disp, baseUrl);
                })
                .ToList();

            return new DataPagedResult<AtraccionListadoResponse>(
                responses, paged.TotalFiltrado, paged.TotalSinFiltros, paged.Page, paged.Limit);
        }

        public async Task<AtraccionDetalleResponse> ObtenerPorGuidAsync(Guid atGuid, string baseUrl)
        {
            var model = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            var disponibilidad = await _ticketService.ObtenerDisponibilidadAsync(model.AtId);
            var tickets  = (await _ticketService.ListarPorAtraccionAsync(model.AtId)).ToList();
            var horarios = (await _ticketService.ListarHorariosPorAtraccionAsync(model.AtId)).ToList();

            return AtraccionPublicMapper.ToDetalleResponse(
                model, disponibilidad, tickets, horarios, baseUrl, model.DesNombre);
        }

        public async Task<FiltrosAtraccionResponse> ObtenerFiltrosAsync()
        {
            // #5: Una sola carga — categorías, etiquetas e idiomas se derivan de esta lista
            var atracciones = (await _atraccionService.ListarConFiltrosAsync(
                new AtraccionFiltroDataModel { Limit = 1000 })).Items;

            var total = atracciones.Count;
            var destinos = await _destinoService.ListarActivosAsync();
            var categoriasActivas = await _categoriaService.ListarActivasAsync();
            var idiomasActivos = await _idiomaService.ListarActivosAsync();

            // #8: Count real por cada destino — ya no siempre 0 para los que no son la ciudad buscada
            var destinationFilters = destinos
                .Select(destino => new OpcionFiltroResponse
                {
                    Name         = destino.DesNombre,
                    Tagname      = destino.DesGuid.ToString(),
                    ProductCount = atracciones.Count(a => a.DesId == destino.DesId),
                    Image        = destino.DesImagenUrl is not null
                                   ? new ImagenFiltroResponse { Url = destino.DesImagenUrl }
                                   : null
                })
                .ToList();

            // #5: Todos los helpers usan la misma lista cargada (excepto categorías que usa query optimizada)
            // E-03: Obtener estructura jerárquica real desde BD y contar atracciones
            var categoriasBD = categoriasActivas.Where(c => c.CatParentId is null).ToList();
            var categorias = new List<OpcionFiltroResponse>();
            foreach (var cat in categoriasBD)
            {
                var hijosCatalogo = categoriasActivas.Where(h => h.CatParentId == cat.CatId).ToList();
                var hijos = hijosCatalogo.Select(h => new OpcionFiltroResponse
                {
                    Name = h.CatNombre,
                    Tagname = h.CatGuid.ToString(),
                    ProductCount = atracciones.Count(a => a.Categorias.Any(ac => ac.CatId == h.CatId)),
                    Image = null
                }).ToList();

                var productCount = atracciones.Count(a =>
                    a.Categorias.Any(ac => ac.CatId == cat.CatId || hijosCatalogo.Any(h => h.CatId == ac.CatId)));

                categorias.Add(new OpcionFiltroResponse
                {
                    Name = cat.CatNombre,
                    Tagname = cat.CatGuid.ToString(),
                    ProductCount = productCount,
                    Image = null,
                    ChildFilterOptions = hijos.Any() ? hijos : null
                });
            }

            var etiquetasConteo = atracciones
                .SelectMany(a => a.Incluyes)
                .Where(i => !i.IncDescripcion.StartsWith("NO:"))
                .GroupBy(i => i.IncDescripcion)
                .Select(g => (Descripcion: g.Key, Conteo: g.Count()))
                .ToList();

            var idiomasConteo = idiomasActivos
                .Select(i => (
                    Descripcion: i.IdDescripcion,
                    Guid: i.IdGuid,
                    Conteo: atracciones.Count(a => a.Idiomas.Any(ai => ai.IdId == i.IdId))))
                .ToList();

            // #2: MinRatingFilter — counts reales basados en calificación promedio de la ciudad
            var minRatingFilters = new[] { 4.5, 4.0, 3.5, 3.0 }
                .Select(r => new OpcionFiltroResponse
                {
                    Name         = $"{r:F1} y más",
                    Tagname      = $"{r:F1}",
                    ProductCount = atracciones.Count(a =>
                        a.CalificacionPromedio.HasValue &&
                        a.CalificacionPromedio.Value >= r)
                }).ToList();

            // #2 + #10: TimeOfDayFilters — los horarios no están en AtraccionDataModel
            // (requeriría una query adicional por atracción — se incluye la estructura con count real
            //  en versión futura cuando se agregue HorariosResumen al data model)
            var timeOfDayFilters = new List<OpcionFiltroResponse>
            {
                new() { Name = "Mañanas", Tagname = "05:00-12:00", ProductCount = 0 },
                new() { Name = "Tardes",  Tagname = "12:00-18:00", ProductCount = 0 },
                new() { Name = "Noches",  Tagname = "18:00-05:00", ProductCount = 0 },
            };

            return new FiltrosAtraccionResponse
            {
                DestinationFilters = destinationFilters,
                TypeFilters = categorias,
                LabelFilters = etiquetasConteo.Select(e => new OpcionFiltroResponse
                {
                    Name = e.Descripcion, Tagname = e.Descripcion, ProductCount = e.Conteo
                }).ToList(),
                MinRatingFilter = minRatingFilters,
                TimeOfDayFilters = timeOfDayFilters,
                SupportedLanguageFilters = idiomasConteo.Select(i => new OpcionFiltroResponse
                {
                    Name = i.Descripcion, Tagname = i.Guid.ToString(), ProductCount = i.Conteo
                }).ToList(),
                UfiFilters = new List<OpcionFiltroResponse>
                {
                    new()
                    {
                        Name = "Todos",
                        Tagname = "todos",
                        ProductCount = total
                    }
                }
            };
        }

        public async Task<IReadOnlyList<TicketDisponibleResponse>> ListarTicketsAsync(Guid atGuid)
        {
            var atraccion = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            var horarios = (await _ticketService.ListarHorariosPorAtraccionAsync(atraccion.AtId, 365))
                .Where(EsHorarioDisponible)
                .ToList();

            return (await _ticketService.ListarPorAtraccionAsync(atraccion.AtId))
                .Where(t => t.TckEstado == 'A')
                .Select(t => ToTicketDisponibleResponse(t, horarios.Where(h => h.TckId == t.TckId)))
                .ToList();
        }

        public async Task<IReadOnlyList<HorarioProximoResponse>> ListarHorariosPorTicketAsync(Guid tckGuid)
        {
            var ticket = await _ticketService.ObtenerPorGuidAsync(tckGuid)
                ?? throw new NotFoundException("Ticket", tckGuid);

            return (await _ticketService.ListarHorariosPorTicketAsync(ticket.TckId))
                .Where(EsHorarioDisponible)
                .Select(ToHorarioProximoResponse)
                .ToList();
        }

        public async Task<IReadOnlyList<HorarioProximoResponse>> ListarHorariosDisponiblesAsync(Guid atGuid)
        {
            var atraccion = await _atraccionService.ObtenerPorGuidAsync(atGuid)
                ?? throw new NotFoundException("Atraccion", atGuid);

            return (await _ticketService.ListarHorariosPorAtraccionAsync(atraccion.AtId, 365))
                .Where(EsHorarioDisponible)
                .Select(ToHorarioProximoResponse)
                .ToList();
        }

        private static TicketDisponibleResponse ToTicketDisponibleResponse(
            TicketDataModel ticket,
            IEnumerable<HorarioDataModel> horarios)
            => new()
            {
                TckGuid = ticket.TckGuid.ToString(),
                Titulo = ticket.TckTitulo,
                Tipo = ticket.TckTipoParticipante,
                Precio = ticket.TckPrecio,
                CuposDisponibles = ticket.TckCuposDisponibles,
                HorariosDisponibles = horarios.Select(ToHorarioProximoResponse).ToList()
            };

        private static HorarioProximoResponse ToHorarioProximoResponse(HorarioDataModel horario)
            => new()
            {
                HorGuid = horario.HorGuid.ToString(),
                TckGuid = horario.TckGuid == Guid.Empty ? string.Empty : horario.TckGuid.ToString(),
                TicketTitulo = horario.TckTitulo,
                Fecha = horario.HorFecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                HoraInicio = horario.HorHoraInicio.ToString("HH:mm", CultureInfo.InvariantCulture),
                HoraFin = horario.HorHoraFin?.ToString("HH:mm", CultureInfo.InvariantCulture),
                Cupos = horario.HorCuposDisponibles,
                Disponible = EsHorarioDisponible(horario)
            };

        private static bool EsHorarioDisponible(HorarioDataModel horario)
            => horario.HorEstado == 'A'
               && horario.HorCuposDisponibles > 0
               && horario.HorFecha >= DateOnly.FromDateTime(DateTime.UtcNow);

    }
}
