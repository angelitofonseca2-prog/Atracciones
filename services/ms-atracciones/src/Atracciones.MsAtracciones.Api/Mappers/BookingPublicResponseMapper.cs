using Atracciones.MsAtracciones.Business.Dtos.Public.Atracciones;

namespace Atracciones.MsAtracciones.Api.Mappers;

/// <summary>Proyecciones JSON alineadas al contrato Booking PDF v2.</summary>
public static class BookingPublicResponseMapper
{
    public static object ToTicketSimple(TicketDisponibleResponse t) => new
    {
        tck_guid = t.TckGuid,
        tipo = t.Tipo,
        precio = t.Precio,
        moneda = t.Moneda,
    };

    public static object ToHorarioSimple(HorarioProximoResponse h) => new
    {
        hor_guid = h.HorGuid,
        fecha = h.Fecha,
        fecha_fin = h.FechaFin,       // necesario para calcular el rango del calendario
        hora_inicio = h.HoraInicio,
        hora_fin = h.HoraFin,
        cupos = h.Cupos,
        cupos_disponibles = h.Cupos,  // alias para compatibilidad con el frontend
    };

    public static object ToDetalleBooking(AtraccionDetalleResponse d) => new
    {
        d.Id,
        d.Nombre,
        d.Ciudad,
        d.Pais,
        d.TipoTagname,
        d.TipoNombre,
        d.SubtipoTagname,
        d.SubtipoNombre,
        d.Etiquetas,
        d.DescripcionCorta,
        d.ImagenPrincipal,
        d.DuracionMinutos,
        d.PrecioDesde,
        d.Moneda,
        d.Calificacion,
        d.TotalResenas,
        d.IdiomasDisponibles,
        d.Disponibilidad,
        descripcion = d.Descripcion,
        imagenes = d.Imagenes,
        incluye = d.Incluye,
        no_incluye = d.NoIncluye,
        punto_encuentro = d.PuntoEncuentro,
        incluye_transporte = d.IncluyeTransporte,
        incluye_acompaniante = d.IncluyeAcompaniante,
        tickets = d.Tickets.Select(t => new { tck_guid = t.TckGuid, tipo = t.Tipo, precio = t.Precio, moneda = t.Moneda }).ToList(),
        horarios_proximos = d.HorariosProximos.Select(h => new
        {
            fecha = h.Fecha,
            hora_inicio = h.HoraInicio,
            hora_fin = h.HoraFin,
            cupos = h.Cupos,
        }).ToList(),
        d.Links,
    };
}
