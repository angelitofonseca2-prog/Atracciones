using Microservicio.Atracciones.Business.DTOs.Public.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Atracciones;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microservicio.Atracciones.DataManagement.Models.Reservas;

namespace Microservicio.Atracciones.Business.Mappers.Public
{
    public static class AtraccionPublicMapper
    {
        /// <summary>
        /// Construye AtraccionListadoResponse desde el DataModel.
        /// Aplica las reglas del contrato: descripcion_corta truncada a 150,
        /// tipo/subtipo desde la jerarquía de CATEGORIA, HATEOAS links.
        /// </summary>
        public static AtraccionListadoResponse ToListadoResponse(
            AtraccionDataModel model,
            (bool DisponibleHoy, DateOnly? ProximaFecha, int? Cupos) disponibilidad,
            string baseUrl)
        {
            var (tipoGuid, tipoNombre, subtipoGuid, subtipoNombre) = ResolverTipoSubtipo(model.Categorias);

            return new AtraccionListadoResponse
            {
                Id = model.AtGuid.ToString(),
                Nombre = model.AtNombre,
                Ciudad = model.DesNombre,
                Pais = model.DesPais,
                TipoTagname = tipoGuid,
                TipoNombre = tipoNombre,
                SubtipoTagname = subtipoGuid,
                SubtipoNombre = subtipoNombre,
                Etiquetas = model.Incluyes
                                          .Where(i => !i.IncDescripcion.StartsWith("NO:"))
                                          .Select(i => i.IncDescripcion)
                                          .ToList(),
                DescripcionCorta = model.AtDescripcion is not null
                                       ? model.AtDescripcion[..Math.Min(150, model.AtDescripcion.Length)]
                                       : string.Empty,
                ImagenPrincipal = model.Imagenes.FirstOrDefault()?.ImgUrl,  // #11: null si no hay imagen, no string.Empty
                DuracionMinutos = model.AtDuracionMinutos,
                PrecioDesde = model.PrecioDesde ?? 0,
                Moneda = "USD",
                Calificacion = model.CalificacionPromedio.HasValue
                                       ? Math.Round(model.CalificacionPromedio.Value, 1)
                                       : 0.0,
                TotalResenas = model.AtTotalResenias,
                IdiomasDisponibles = model.Idiomas.Select(i => i.IdDescripcion).ToList(),
                Disponibilidad = new DisponibilidadResponse
                {
                    Disponible = model.AtDisponible && model.AtEstado == 'A',
                    DisponibleHoy = disponibilidad.DisponibleHoy,
                    ProximaFechaDisponible = disponibilidad.ProximaFecha?.ToString("yyyy-MM-dd"),
                    CuposDisponibles = disponibilidad.Cupos
                },
                Links = new Dictionary<string, string?>
                {
                    ["self"] = $"{baseUrl}/api/v1/atracciones/{model.AtGuid}",
                    ["detalle"] = $"{baseUrl}/api/v1/atracciones/{model.AtGuid}"
                }
            };
        }

        /// <summary>
        /// Construye AtraccionDetalleResponse — extiende el listado con
        /// campos del contrato sección 3.3: incluye, no_incluye, tickets,
        /// horarios_proximos y galería completa.
        /// </summary>
        public static AtraccionDetalleResponse ToDetalleResponse(
            AtraccionDataModel model,
            (bool DisponibleHoy, DateOnly? ProximaFecha, int? Cupos) disponibilidad,
            IList<TicketDataModel> tickets,
            IList<HorarioDataModel> horariosProximos,
            string baseUrl,
            string? ciudad = null)
        {
            var listado = ToListadoResponse(model, disponibilidad, baseUrl);

            var detalle = new AtraccionDetalleResponse
            {
                // Campos heredados del listado
                Id = listado.Id,
                Nombre = listado.Nombre,
                Ciudad = listado.Ciudad,
                Pais = listado.Pais,
                TipoTagname = listado.TipoTagname,
                TipoNombre = listado.TipoNombre,
                SubtipoTagname = listado.SubtipoTagname,
                SubtipoNombre = listado.SubtipoNombre,
                Etiquetas = listado.Etiquetas,
                DescripcionCorta = listado.DescripcionCorta,
                ImagenPrincipal = listado.ImagenPrincipal,
                DuracionMinutos = listado.DuracionMinutos,
                PrecioDesde = listado.PrecioDesde,
                Moneda = listado.Moneda,
                Calificacion = listado.Calificacion,
                TotalResenas = listado.TotalResenas,
                IdiomasDisponibles = listado.IdiomasDisponibles,
                Disponibilidad = listado.Disponibilidad,

                // Campos exclusivos del detalle
                Descripcion = model.AtDescripcion ?? string.Empty,
                Imagenes = model.Imagenes.Select(i => i.ImgUrl).ToList(),
                Incluye = model.Incluyes
                                          .Where(i => !i.IncDescripcion.StartsWith("NO:"))
                                          .Select(i => i.IncDescripcion)
                                          .ToList(),
                NoIncluye = model.Incluyes
                                          .Where(i => i.IncDescripcion.StartsWith("NO:"))
                                          .Select(i => i.IncDescripcion[3..])  // quita "NO:"
                                          .ToList(),
                PuntoEncuentro = model.AtPuntoEncuentro,
                IncluyeTransporte = model.AtIncluyeTransporte,
                IncluyeAcompaniante = model.AtIncluyeAcompaniante,

                Tickets = tickets.Select(t => new TicketDisponibleResponse
                {
                    TckGuid = t.TckGuid.ToString(),
                    Tipo = t.TckTipoParticipante,
                    Precio = t.TckPrecio,
                    Moneda = "USD"
                }).ToList(),

                HorariosProximos = horariosProximos.Select(h => new HorarioProximoResponse
                {
                    Fecha = h.HorFecha.ToString("yyyy-MM-dd"),
                    HoraInicio = h.HorHoraInicio.ToString("HH:mm"),
                    HoraFin = h.HorHoraFin?.ToString("HH:mm"),
                    Cupos = h.HorCuposDisponibles
                }).ToList(),

                Links = new Dictionary<string, string?>
                {
                    ["self"] = $"{baseUrl}/api/v1/atracciones/{model.AtGuid}",
                    ["listado"] = ciudad is not null
                                  ? $"{baseUrl}/api/v1/atracciones?ciudad={ciudad}"
                                  : $"{baseUrl}/api/v1/atracciones"
                }
            };

            return detalle;
        }

        // Resuelve tipo (raíz) y subtipo (hijo) desde la lista de categorías
        private static (string TipoGuid, string TipoNombre, string? SubtipoGuid, string? SubtipoNombre)
            ResolverTipoSubtipo(IReadOnlyList<CategoriaDataModel> categorias)
        {
            // Categorías raíz = sin parent
            var raiz = categorias.FirstOrDefault(c => c.CatParentId is null);
            // Categorías hijo = con parent
            var hijo = categorias.FirstOrDefault(c => c.CatParentId is not null);

            if (raiz is null && hijo?.ParentGuid is not null)
            {
                return (
                    hijo.ParentGuid.Value.ToString(),
                    hijo.ParentNombre ?? string.Empty,
                    hijo.CatGuid.ToString(),
                    hijo.CatNombre
                );
            }

            return (
                raiz?.CatGuid.ToString() ?? string.Empty,
                raiz?.CatNombre ?? string.Empty,
                hijo?.CatGuid.ToString(),
                hijo?.CatNombre
            );
        }
    }
}
