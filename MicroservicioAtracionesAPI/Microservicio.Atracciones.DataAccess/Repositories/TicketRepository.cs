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
    public class TicketRepository : ITicketRepository
    {
        private readonly AtraccionesDbContext _context;
        public TicketRepository(AtraccionesDbContext context) => _context = context;

        public async Task<TicketEntity?> ObtenerPorIdAsync(int tckId)
            => await _context.Tickets
                .Include(x => x.Atraccion)
                .Include(x => x.Horarios)
                .FirstOrDefaultAsync(x => x.TckId == tckId && x.TckEstado == 'A');

        public async Task<TicketEntity?> ObtenerPorGuidAsync(Guid tckGuid)
            => await _context.Tickets
                .Include(x => x.Atraccion)
                .Include(x => x.Horarios)
                .FirstOrDefaultAsync(x => x.TckGuid == tckGuid && x.TckEstado == 'A');

        public async Task<IReadOnlyList<TicketEntity>> ListarActivosAsync()
            => await _context.Tickets
                .AsNoTracking()
                .Include(x => x.Atraccion)
                .Include(x => x.Horarios)
                .Where(x => x.TckEstado == 'A')
                .OrderBy(x => x.Atraccion.AtNombre)
                .ThenBy(x => x.TckTitulo)
                .ToListAsync();

        public async Task<IReadOnlyList<TicketEntity>> ListarPorAtraccionAsync(int atId)
            => await _context.Tickets
                .AsNoTracking()
                .Include(x => x.Atraccion)
                .Include(x => x.Horarios)
                .Where(x => x.AtId == atId && x.TckEstado == 'A')
                .ToListAsync();

        public async Task AgregarAsync(TicketEntity ticket)
            => await _context.Tickets.AddAsync(ticket);

        public void Actualizar(TicketEntity ticket)
            => _context.Tickets.Update(ticket);

        public async Task<HorarioEntity?> ObtenerHorarioPorIdAsync(int horId)
            => await _context.Horarios
                .Include(x => x.Ticket).ThenInclude(t => t.Atraccion)
                .FirstOrDefaultAsync(x => x.HorId == horId && x.HorEstado == 'A');

        public async Task<HorarioEntity?> ObtenerHorarioPorGuidAsync(Guid horGuid)
            => await _context.Horarios
                .Include(x => x.Ticket).ThenInclude(t => t.Atraccion)
                .FirstOrDefaultAsync(x => x.HorGuid == horGuid && x.HorEstado == 'A');

        public async Task<IReadOnlyList<HorarioEntity>> ListarHorariosActivosAsync()
            => await _context.Horarios
                .AsNoTracking()
                .Include(x => x.Ticket).ThenInclude(t => t.Atraccion)
                .Where(x => x.HorEstado == 'A')
                .OrderBy(x => x.HorFecha)
                .ThenBy(x => x.HorHoraInicio)
                .ToListAsync();

        public async Task<IReadOnlyList<HorarioEntity>> ListarHorariosPorTicketAsync(int tckId)
            => await _context.Horarios
                .AsNoTracking()
                .Include(x => x.Ticket).ThenInclude(t => t.Atraccion)
                .Where(x => x.TckId == tckId && x.HorEstado == 'A')
                .OrderBy(x => x.HorFecha)
                .ThenBy(x => x.HorHoraInicio)
                .ToListAsync();

        public async Task AgregarHorarioAsync(HorarioEntity horario)
            => await _context.Horarios.AddAsync(horario);

        public void ActualizarHorario(HorarioEntity horario)
            => _context.Horarios.Update(horario);
    }
}
