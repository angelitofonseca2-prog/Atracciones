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
    public class ClienteRepository : IClienteRepository
    {
        private readonly AtraccionesDbContext _context;
        public ClienteRepository(AtraccionesDbContext context) => _context = context;

        public async Task<ClienteEntity?> ObtenerPorIdAsync(int cliId)
            => await _context.Clientes
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.CliId == cliId && x.CliEstado == 'A');

        public async Task<ClienteEntity?> ObtenerPorGuidAsync(Guid cliGuid)
            => await _context.Clientes
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.CliGuid == cliGuid && x.CliEstado == 'A');

        public async Task<ClienteEntity?> ObtenerPorUsuarioIdAsync(int usuId)
            => await _context.Clientes
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.UsuId == usuId && x.CliEstado == 'A');

        public async Task<ClienteEntity?> ObtenerPorNumeroIdentificacionAsync(string numeroIdentificacion)
            => await _context.Clientes
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.CliNumeroIdentificacion == numeroIdentificacion && x.CliEstado == 'A');

        public async Task<IReadOnlyList<ClienteEntity>> ListarAsync()
            => await _context.Clientes
                .AsNoTracking()
                .Where(x => x.CliEstado == 'A')
                .ToListAsync();

        public async Task AgregarAsync(ClienteEntity cliente)
            => await _context.Clientes.AddAsync(cliente);

        public void Actualizar(ClienteEntity cliente)
            => _context.Clientes.Update(cliente);
    }
}
