using Microsoft.EntityFrameworkCore;
using Microservicio.Atracciones.DataAccess.Common;
using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microservicio.Atracciones.DataAccess.Entities.Facturacion;

namespace Microservicio.Atracciones.DataAccess.Queries
{
    // ====================================================================
    //  FACTURA QUERY REPOSITORY
    // ====================================================================

    /// <summary>
    /// Consultas de facturación y reportes administrativos.
    /// </summary>
    public class FacturaQueryRepository
    {
        private readonly AtraccionesDbContext _context;

        public FacturaQueryRepository(AtraccionesDbContext context)
            => _context = context;

        // ----------------------------------------------------------------
        //  Listado paginado de facturas (panel admin)
        // ----------------------------------------------------------------
        public async Task<PagedResult<FacturaEntity>> ListarAdminAsync(int page, int limit, char? estado = null)
        {
            var query = _context.Facturas
                .AsNoTracking()
                .Include(x => x.Reserva).ThenInclude(r => r.Cliente)
                .Include(x => x.DatosFacturacion)
                .AsQueryable();

            if (estado.HasValue)
                query = query.Where(x => x.FacEstado == estado.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.FacFechaEmision)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return new PagedResult<FacturaEntity>(items, total, total, page, limit);
        }

        public async Task<PagedResult<FacturaEntity>> ListarPorClienteAsync(int cliId, int page, int limit)
        {
            var query = _context.Facturas
                .AsNoTracking()
                .Include(x => x.Reserva)
                .Include(x => x.DatosFacturacion)
                .Where(x => x.Reserva.CliId == cliId && x.FacEstado == 'A');

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.FacFechaEmision)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            return new PagedResult<FacturaEntity>(items, total, total, page, limit);
        }

        // ----------------------------------------------------------------
        //  Factura completa con datos del receptor
        // ----------------------------------------------------------------
        public async Task<FacturaEntity?> ObtenerFacturaCompletaAsync(Guid facGuid)
            => await _context.Facturas
                .AsNoTracking()
                .Include(x => x.Reserva)
                    .ThenInclude(r => r.Cliente)
                .Include(x => x.Reserva)
                    .ThenInclude(r => r.ReservasDetalle)
                        .ThenInclude(d => d.Ticket)
                            .ThenInclude(t => t.Atraccion)
                .Include(x => x.DatosFacturacion)
                .FirstOrDefaultAsync(x => x.FacGuid == facGuid);

        // ----------------------------------------------------------------
        //  Suma de facturación por rango de fechas (reporte simple)
        // ----------------------------------------------------------------
        public async Task<decimal> SumarTotalFacturadoAsync(DateTime desde, DateTime hasta)
            => await _context.Facturas
                .AsNoTracking()
                .Where(x =>
                    x.FacEstado == 'A' &&
                    x.FacFechaEmision >= desde &&
                    x.FacFechaEmision <= hasta)
                .SumAsync(x => x.FacTotal);

        // ----------------------------------------------------------------
        //  Factura por número de factura
        // ----------------------------------------------------------------
        public async Task<FacturaEntity?> ObtenerPorNumeroAsync(string numero)
            => await _context.Facturas
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FacNumero == numero && f.FacEstado == 'A');

        public async Task<int> ObtenerMaxSecuencialPorAnioAsync(int anio)
        {
            var prefijo = $"FAC-{anio}-";
            var numeros = await _context.Facturas
                .AsNoTracking()
                .Where(f => f.FacNumero.StartsWith(prefijo))
                .Select(f => f.FacNumero)
                .ToListAsync();

            return numeros
                .Select(numero =>
                {
                    var secuencial = numero.Length > prefijo.Length
                        ? numero[prefijo.Length..]
                        : string.Empty;

                    return int.TryParse(secuencial, out var valor) ? valor : 0;
                })
                .DefaultIfEmpty(0)
                .Max();
        }
    }
}
