using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microsoft.EntityFrameworkCore;
using Microservicio.Atracciones.DataAccess.Common;

namespace Microservicio.Atracciones.DataAccess.Queries
{
    // ====================================================================
    //  TICKET QUERY REPOSITORY
    // ====================================================================

    /// <summary>
    /// Consultas especializadas de disponibilidad: slots próximos,
    /// cupos en tiempo real y verificación de capacidad antes de reservar.
    /// La disponibilidad NUNCA usa caché — se consulta directo a HORARIO.
    /// </summary>
    public class TicketQueryRepository
    {
        private readonly AtraccionesDbContext _context;

        public TicketQueryRepository(AtraccionesDbContext context)
            => _context = context;

        // ----------------------------------------------------------------
        //  Próximos N días de horarios disponibles para una atracción
        //  (usado en el detalle público → campo horarios_proximos)
        // ----------------------------------------------------------------
        public async Task<List<HorarioEntity>> ObtenerHorariosPorAtraccionAsync(int atId, int diasAdelante = 7)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var limite = hoy.AddDays(diasAdelante);

            return await _context.Horarios
                .AsNoTracking()
                .Include(x => x.Ticket).ThenInclude(t => t.Atraccion)
                .Where(x =>
                    x.Ticket.AtId == atId &&
                    x.Ticket.TckEstado == 'A' &&
                    x.HorEstado == 'A' &&
                    x.HorFecha >= hoy &&
                    x.HorFecha <= limite &&
                    x.HorCuposDisponibles > 0)
                .OrderBy(x => x.HorFecha)
                .ThenBy(x => x.HorHoraInicio)
                .ToListAsync();
        }

        // ----------------------------------------------------------------
        //  Objeto de disponibilidad en tiempo real para el listado/detalle
        //  (contrato: disponibilidad.disponible_hoy, proxima_fecha, cupos)
        // ----------------------------------------------------------------
        public async Task<(bool DisponibleHoy, DateOnly? ProximaFecha, int? CuposProximos)>
            ObtenerDisponibilidadAsync(int atId)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            var horariosActivos = await _context.Horarios
                .AsNoTracking()
                .Include(x => x.Ticket).ThenInclude(t => t.Atraccion)
                .Where(x =>
                    x.Ticket.AtId == atId &&
                    x.Ticket.TckEstado == 'A' &&
                    x.HorEstado == 'A' &&
                    x.HorFecha >= hoy &&
                    x.HorCuposDisponibles > 0)
                .OrderBy(x => x.HorFecha)
                .ThenBy(x => x.HorHoraInicio)
                .ToListAsync();

            var hayHoy = horariosActivos.Any(h => h.HorFecha == hoy);
            var primero = horariosActivos.FirstOrDefault();
            var proxFecha = primero?.HorFecha;
            var cupos = primero?.HorCuposDisponibles;

            // proxima_fecha_disponible = null si el primer slot es hoy mismo
            return (hayHoy, proxFecha == hoy ? null : proxFecha, cupos);
        }

        // ----------------------------------------------------------------
        //  Verifica cupos disponibles antes de confirmar una reserva
        //  (usado por Business/ReservaRules antes de INSERT)
        // ----------------------------------------------------------------
        public async Task<(bool HayCupos, int CuposActuales)>
            VerificarCuposAsync(int horId, int cantidadSolicitada)
        {
            var horario = await _context.Horarios
                .FirstOrDefaultAsync(x => x.HorId == horId && x.HorEstado == 'A');

            if (horario is null)
                return (false, 0);

            return (horario.HorCuposDisponibles >= cantidadSolicitada, horario.HorCuposDisponibles);
        }

        // ----------------------------------------------------------------
        //  #4: Batch de disponibilidad — una sola query para N atracciones
        //  Devuelve dictionary<atId, (DisponibleHoy, ProximaFecha, Cupos)>
        // ----------------------------------------------------------------
        public async Task<Dictionary<int, (bool DisponibleHoy, DateOnly? ProximaFecha, int? CuposProximos)>>
            ObtenerDisponibilidadBatchAsync(IEnumerable<int> atIds)
        {
            var hoy    = DateOnly.FromDateTime(DateTime.UtcNow);
            var idsSet = atIds.ToHashSet();

            var horarios = await _context.Horarios
                .AsNoTracking()
                .Include(x => x.Ticket).ThenInclude(t => t.Atraccion)
                .Where(x =>
                    idsSet.Contains(x.Ticket.AtId) &&
                    x.Ticket.TckEstado == 'A' &&
                    x.HorEstado == 'A' &&
                    x.HorFecha >= hoy &&
                    x.HorCuposDisponibles > 0)
                .OrderBy(x => x.HorFecha)
                .ThenBy(x => x.HorHoraInicio)
                .ToListAsync();

            var resultado = new Dictionary<int, (bool, DateOnly?, int?)>();

            foreach (var atId in idsSet)
            {
                var deEsta  = horarios.Where(h => h.Ticket.AtId == atId).ToList();
                var hayHoy  = deEsta.Any(h => h.HorFecha == hoy);
                var primero = deEsta.FirstOrDefault();
                var proxFecha = primero?.HorFecha;
                var cupos     = primero?.HorCuposDisponibles;

                resultado[atId] = (hayHoy, proxFecha == hoy ? null : proxFecha, cupos);
            }

            return resultado;
        }

        // ----------------------------------------------------------------
        //  Tickets activos de una atracción con su precio mínimo
        //  (para campo precio_desde del listado)
        // ----------------------------------------------------------------
        public async Task<decimal?> ObtenerPrecioDesdeAsync(int atId)
            => await _context.Tickets
                .AsNoTracking()
                .Where(x => x.AtId == atId && x.TckEstado == 'A')
                .MinAsync(x => (decimal?)x.TckPrecio);

        public async Task<bool> TieneReservasActivasPorTicketAsync(int tckId)
            => await _context.ReservasDetalle
                .AsNoTracking()
                .AnyAsync(d => d.TckId == tckId && d.RdetEstado == 'A' && d.Reserva.RevEstado == 'A');

        public async Task<bool> TieneReservasActivasPorHorarioAsync(int horId)
            => await _context.Reservas
                .AsNoTracking()
                .AnyAsync(r => r.HorId == horId && r.RevEstado == 'A');
    }
}
