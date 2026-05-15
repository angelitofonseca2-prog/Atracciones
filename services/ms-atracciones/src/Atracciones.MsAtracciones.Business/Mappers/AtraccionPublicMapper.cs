using Atracciones.Contracts.Catalogos.V1;
using Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;
using Atracciones.MsAtracciones.DataManagement.Models;

namespace Atracciones.MsAtracciones.Business.Mappers;

public static class AtraccionPublicMapper
{
    public static AtraccionListadoResponse ToListadoResponse(
        AtraccionIndexRow row,
        IReadOnlyList<CategoriaGrpc> categoriasDeAtraccion,
        string baseUrl)
    {
        var (tipoGuid, tipoNombre, subtipoGuid, subtipoNombre) = ResolverTipoSubtipo(categoriasDeAtraccion);

        return new AtraccionListadoResponse
        {
            Id = row.AtGuid.ToString(),
            Nombre = row.AtNombre,
            Ciudad = row.DesNombre,
            Pais = row.DesPais,
            TipoTagname = tipoGuid,
            TipoNombre = tipoNombre,
            SubtipoTagname = subtipoGuid,
            SubtipoNombre = subtipoNombre,
            Etiquetas = row.IncluyeSnaps.Where(i => !i.StartsWith("NO:", StringComparison.Ordinal)).ToList(),
            DescripcionCorta = row.AtDescripcion is not null
                ? row.AtDescripcion[..Math.Min(150, row.AtDescripcion.Length)]
                : string.Empty,
            ImagenPrincipal = row.ImagenPrincipal,
            DuracionMinutos = row.AtDuracionMinutos,
            PrecioDesde = row.PrecioMinimoTickets,
            Moneda = "USD",
            Calificacion = row.CalificacionPromedio.HasValue ? Math.Round(row.CalificacionPromedio.Value, 1) : 0.0,
            TotalResenas = row.AtTotalResenias,
            IdiomasDisponibles = row.IdiomaSnaps.ToList(),
            Disponibilidad = new DisponibilidadResponse
            {
                Disponible = row.AtDisponible,
                DisponibleHoy = row.DispDisponibleHoy,
                ProximaFechaDisponible = row.DispProximaFecha?.ToString("yyyy-MM-dd"),
                CuposDisponibles = row.DispCupos,
            },
            Links = new Dictionary<string, string?>
            {
                ["self"] = $"{baseUrl}/api/v1/atracciones/{row.AtGuid}",
                ["detalle"] = $"{baseUrl}/api/v1/atracciones/{row.AtGuid}",
            },
        };
    }

    public static AtraccionDetalleResponse ToDetalleResponse(
        AtraccionDetalleRow m,
        IReadOnlyList<CategoriaGrpc> categoriasDeAtraccion,
        (bool DisponibleHoy, DateOnly? ProximaFecha, int? Cupos) disponibilidad,
        IList<TicketDisponibleResponse> tickets,
        IList<HorarioProximoResponse> horariosProximos,
        string baseUrl,
        string? ciudad = null)
    {
        var precioDesde = m.Tickets.Count > 0 ? m.Tickets.Min(t => t.TckPrecio) : 0m;
        var (tipoGuid, tipoNombre, subtipoGuid, subtipoNombre) = ResolverTipoSubtipo(categoriasDeAtraccion);
        var imgPrincipal = m.Imagenes.OrderBy(i => i.Orden).FirstOrDefault()?.ImgUrl;

        var listadoBase = new AtraccionListadoResponse
        {
            Id = m.AtGuid.ToString(),
            Nombre = m.AtNombre,
            Ciudad = m.DesNombre,
            Pais = m.DesPais,
            TipoTagname = tipoGuid,
            TipoNombre = tipoNombre,
            SubtipoTagname = subtipoGuid,
            SubtipoNombre = subtipoNombre,
            Etiquetas = m.Incluyes.Where(i => !i.IncDescripcion.StartsWith("NO:")).Select(i => i.IncDescripcion).ToList(),
            DescripcionCorta = m.AtDescripcion is not null
                ? m.AtDescripcion[..Math.Min(150, m.AtDescripcion.Length)]
                : string.Empty,
            ImagenPrincipal = imgPrincipal,
            DuracionMinutos = m.AtDuracionMinutos,
            PrecioDesde = precioDesde,
            Moneda = "USD",
            Calificacion = m.CalificacionPromedio.HasValue ? Math.Round(m.CalificacionPromedio.Value, 1) : 0.0,
            TotalResenas = m.AtTotalResenias,
            IdiomasDisponibles = m.Idiomas.Select(i => i.IdDescripcion).ToList(),
            Disponibilidad = new DisponibilidadResponse
            {
                Disponible = m.AtDisponible,
                DisponibleHoy = disponibilidad.DisponibleHoy,
                ProximaFechaDisponible = disponibilidad.ProximaFecha?.ToString("yyyy-MM-dd"),
                CuposDisponibles = disponibilidad.Cupos,
            },
            Links = new Dictionary<string, string?>
            {
                ["self"] = $"{baseUrl}/api/v1/atracciones/{m.AtGuid}",
                ["detalle"] = $"{baseUrl}/api/v1/atracciones/{m.AtGuid}",
            },
        };

        return new AtraccionDetalleResponse
        {
            Id = listadoBase.Id,
            Nombre = listadoBase.Nombre,
            Ciudad = listadoBase.Ciudad,
            Pais = listadoBase.Pais,
            TipoTagname = listadoBase.TipoTagname,
            TipoNombre = listadoBase.TipoNombre,
            SubtipoTagname = listadoBase.SubtipoTagname,
            SubtipoNombre = listadoBase.SubtipoNombre,
            Etiquetas = listadoBase.Etiquetas,
            DescripcionCorta = listadoBase.DescripcionCorta,
            ImagenPrincipal = listadoBase.ImagenPrincipal,
            DuracionMinutos = listadoBase.DuracionMinutos,
            PrecioDesde = listadoBase.PrecioDesde,
            Moneda = listadoBase.Moneda,
            Calificacion = listadoBase.Calificacion,
            TotalResenas = listadoBase.TotalResenas,
            IdiomasDisponibles = listadoBase.IdiomasDisponibles,
            Disponibilidad = listadoBase.Disponibilidad,
            Descripcion = m.AtDescripcion ?? string.Empty,
            Imagenes = m.Imagenes.OrderBy(i => i.Orden).Select(i => i.ImgUrl).ToList(),
            Incluye = m.Incluyes.Where(i => !i.IncDescripcion.StartsWith("NO:")).Select(i => i.IncDescripcion).ToList(),
            NoIncluye = m.Incluyes.Where(i => i.IncDescripcion.StartsWith("NO:")).Select(i => i.IncDescripcion[3..]).ToList(),
            PuntoEncuentro = m.AtPuntoEncuentro,
            IncluyeTransporte = m.AtIncluyeTransporte,
            IncluyeAcompaniante = m.AtIncluyeAcompaniante,
            Tickets = tickets,
            HorariosProximos = horariosProximos,
            Links = new Dictionary<string, string?>
            {
                ["self"] = $"{baseUrl}/api/v1/atracciones/{m.AtGuid}",
                ["listado"] = ciudad is not null
                    ? $"{baseUrl}/api/v1/atracciones?ciudad={Uri.EscapeDataString(ciudad)}"
                    : $"{baseUrl}/api/v1/atracciones",
            },
        };
    }

    private static (string TipoGuid, string TipoNombre, string? SubtipoGuid, string? SubtipoNombre)
        ResolverTipoSubtipo(IReadOnlyList<CategoriaGrpc> categoriasDeAtraccion)
    {
        var raiz = categoriasDeAtraccion.FirstOrDefault(c => string.IsNullOrWhiteSpace(c.ParentGuid));
        var hijo = categoriasDeAtraccion.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.ParentGuid));

        if (raiz is null && hijo is not null && !string.IsNullOrWhiteSpace(hijo.ParentGuid))
        {
            var padre = categoriasDeAtraccion.FirstOrDefault(c =>
                string.Equals(c.CatGuid, hijo.ParentGuid, StringComparison.OrdinalIgnoreCase));
            return (
                hijo.ParentGuid,
                padre?.Nombre ?? string.Empty,
                hijo.CatGuid,
                hijo.Nombre);
        }

        return (
            raiz?.CatGuid ?? string.Empty,
            raiz?.Nombre ?? string.Empty,
            hijo?.CatGuid,
            hijo?.Nombre);
    }
}
