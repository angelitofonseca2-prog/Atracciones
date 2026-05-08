using Microsoft.EntityFrameworkCore;
using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Seguridad;
using Microservicio.Atracciones.DataAccess.Entities.Clientes;
using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using Microservicio.Atracciones.DataAccess.Entities.Facturacion;
using Microservicio.Atracciones.DataAccess.Entities.Auditoria;
using Microservicio.Atracciones.DataAccess.Repositories.Interfaces;

namespace Microservicio.Atracciones.DataAccess.Repositories
{
    public class ReservaRepository : IReservaRepository
    {
        private readonly AtraccionesDbContext _context;
        public ReservaRepository(AtraccionesDbContext context) => _context = context;

        public async Task<ReservaEntity?> ObtenerPorIdAsync(int revId)
            => await _context.Reservas
                .Include(x => x.Cliente)
                .Include(x => x.Horario).ThenInclude(h => h.Ticket)
                .Include(x => x.ReservasDetalle).ThenInclude(d => d.Ticket)
                .FirstOrDefaultAsync(x => x.RevId == revId);

        public async Task<ReservaEntity?> ObtenerPorGuidAsync(Guid revGuid)
            => await _context.Reservas
                .Include(x => x.Cliente)
                .Include(x => x.Horario).ThenInclude(h => h.Ticket)
                .Include(x => x.ReservasDetalle).ThenInclude(d => d.Ticket)
                .FirstOrDefaultAsync(x => x.RevGuid == revGuid);

        public async Task<ReservaEntity?> ObtenerPorCodigoAsync(string revCodigo)
            => await _context.Reservas
                .FirstOrDefaultAsync(x => x.RevCodigo == revCodigo);

        public async Task<IReadOnlyList<ReservaEntity>> ListarPorClienteAsync(int cliId)
            => await _context.Reservas
                .AsNoTracking()
                .Include(x => x.Horario).ThenInclude(h => h.Ticket).ThenInclude(t => t.Atraccion)
                .Include(x => x.ReservasDetalle)
                .Where(x => x.CliId == cliId)
                .OrderByDescending(x => x.RevFechaReservaUtc)
                .ToListAsync();

        public async Task AgregarAsync(ReservaEntity reserva)
            => await _context.Reservas.AddAsync(reserva);

        public void Actualizar(ReservaEntity reserva)
            => _context.Reservas.Update(reserva);

        public async Task AgregarDetalleAsync(ReservaDetalleEntity detalle)
            => await _context.ReservasDetalle.AddAsync(detalle);
    }
}
