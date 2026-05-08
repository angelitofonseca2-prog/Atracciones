using Microservicio.Atracciones.DataAccess.Common;
using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Atracciones.DataAccess.Queries
{
    // ====================================================================
    //  RESERVA QUERY REPOSITORY
    // ====================================================================

    /// <summary>
    /// Parámetros de filtrado para el panel administrativo de reservas.
    /// </summary>
    public record ReservaFiltroQuery(
        int? CliId = null,
        int? AtId = null,
        char? Estado = null,   // 'A' | 'I' | 'C'
        DateTime? FechaDesde = null,
        DateTime? FechaHasta = null,
        int Page = 1,
        int Limit = 10
    );

    /// <summary>
    /// Consultas complejas sobre RESERVAS: historial del cliente,
    /// panel administrativo con filtros, detalle completo con líneas.
    /// </summary>
    public class ReservaQueryRepository
    {
        private readonly AtraccionesDbContext _context;

        public ReservaQueryRepository(AtraccionesDbContext context)
            => _context = context;

        // ----------------------------------------------------------------
        //  Historial paginado de un cliente
        // ----------------------------------------------------------------
        public async Task<PagedResult<ReservaEntity>> ListarPorClienteAsync(int cliId, int page, int limit)
        {
            var query = _context.Reservas
                .AsNoTracking()
                .Include(x => x.Horario).ThenInclude(h => h.Ticket).ThenInclude(t => t.Atraccion).ThenInclude(a => a.Destino)
                .Include(x => x.ReservasDetalle).ThenInclude(d => d.Ticket)
                .Include(x => x.Factura)
                .Where(x => x.CliId == cliId)
                .OrderByDescending(x => x.RevFechaReservaUtc);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

            return new PagedResult<ReservaEntity>(items, total, total, page, limit);
        }

        // ----------------------------------------------------------------
        //  Panel administrativo: reservas con filtros
        // ----------------------------------------------------------------
        public async Task<PagedResult<ReservaEntity>> ListarAdminAsync(ReservaFiltroQuery filtro)
        {
            var query = _context.Reservas
                .AsNoTracking()
                .Include(x => x.Cliente)
                .Include(x => x.Horario).ThenInclude(h => h.Ticket).ThenInclude(t => t.Atraccion)
                .Include(x => x.ReservasDetalle).ThenInclude(d => d.Ticket)
                .AsQueryable();

            if (filtro.CliId.HasValue)
                query = query.Where(x => x.CliId == filtro.CliId.Value);

            if (filtro.AtId.HasValue)
                query = query.Where(x => x.Horario.Ticket.AtId == filtro.AtId.Value);

            if (filtro.Estado.HasValue)
                query = query.Where(x => x.RevEstado == filtro.Estado.Value);

            if (filtro.FechaDesde.HasValue)
                query = query.Where(x => x.RevFechaReservaUtc >= filtro.FechaDesde.Value);

            if (filtro.FechaHasta.HasValue)
                query = query.Where(x => x.RevFechaReservaUtc <= filtro.FechaHasta.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.RevFechaReservaUtc)
                .Skip((filtro.Page - 1) * filtro.Limit)
                .Take(filtro.Limit)
                .ToListAsync();

            return new PagedResult<ReservaEntity>(items, total, total, filtro.Page, filtro.Limit);
        }

        // ----------------------------------------------------------------
        //  Detalle completo de una reserva (para el cliente o admin)
        // ----------------------------------------------------------------
        public async Task<ReservaEntity?> ObtenerDetalleCompletoAsync(Guid revGuid)
            => await _context.Reservas
                .AsNoTracking()
                .Include(x => x.Cliente)
                .Include(x => x.Horario).ThenInclude(h => h.Ticket).ThenInclude(t => t.Atraccion).ThenInclude(a => a.Destino)
                .Include(x => x.ReservasDetalle).ThenInclude(d => d.Ticket)
                .Include(x => x.Factura).ThenInclude(f => f!.DatosFacturacion)
                .Include(x => x.Resenia)
                .FirstOrDefaultAsync(x => x.RevGuid == revGuid);
    }
}
