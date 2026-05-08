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
    public class ReseniaRepository : IReseniaRepository
    {
        private readonly AtraccionesDbContext _context;
        public ReseniaRepository(AtraccionesDbContext context) => _context = context;

        public async Task<ReseniaEntity?> ObtenerPorIdAsync(int rsnId)
            => await _context.Resenias.FirstOrDefaultAsync(x => x.RsnId == rsnId && x.RsnEstado == 'A');

        public async Task<ReseniaEntity?> ObtenerPorGuidAsync(Guid rsnGuid)
            => await _context.Resenias.FirstOrDefaultAsync(x => x.RsnGuid == rsnGuid && x.RsnEstado == 'A');

        public async Task<ReseniaEntity?> ObtenerPorReservaAsync(int revId)
            => await _context.Resenias.FirstOrDefaultAsync(x => x.RevId == revId);

        public async Task<bool> ExistePorClienteYAtraccionAsync(int cliId, int atId)
            => await _context.Resenias
                .AsNoTracking()
                .AnyAsync(x =>
                    x.RsnEstado == 'A' &&
                    x.AtId == atId &&
                    x.Reserva.CliId == cliId);

        public async Task<IReadOnlyList<ReseniaEntity>> ListarPorAtraccionAsync(int atId)
            => await _context.Resenias
                .AsNoTracking()
                .Where(x => x.AtId == atId && x.RsnEstado == 'A')
                .OrderByDescending(x => x.RsnFechaCreacion)
                .ToListAsync();

        public async Task AgregarAsync(ReseniaEntity resenia)
            => await _context.Resenias.AddAsync(resenia);

        public void Actualizar(ReseniaEntity resenia)
            => _context.Resenias.Update(resenia);
    }
}
