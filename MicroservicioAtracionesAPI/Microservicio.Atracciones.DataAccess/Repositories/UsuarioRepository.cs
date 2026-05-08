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
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AtraccionesDbContext _context;
        public UsuarioRepository(AtraccionesDbContext context) => _context = context;

        public async Task<UsuarioEntity?> ObtenerPorIdAsync(int usuId)
            => await _context.Usuarios
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.RolEntity)
                .Include(u => u.Cliente) // E-05: incluye el cliente para extraer CliId
                .FirstOrDefaultAsync(x => x.UsuId == usuId && x.UsuEstado == 'A');

        public async Task<UsuarioEntity?> ObtenerPorLoginAsync(string login)
            => await _context.Usuarios
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.RolEntity)
                .Include(u => u.Cliente) // E-05: incluye el cliente para extraer CliId
                .FirstOrDefaultAsync(x => x.UsuLogin == login && x.UsuEstado == 'A');

        public async Task<UsuarioEntity?> ObtenerPorGuidAsync(Guid usuGuid)
            => await _context.Usuarios
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.RolEntity)
                .Include(u => u.Cliente)
                .FirstOrDefaultAsync(x => x.UsuGuid == usuGuid && x.UsuEstado == 'A');

        public async Task<IReadOnlyList<UsuarioEntity>> ListarAsync()
            => await _context.Usuarios
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.RolEntity)
                .Where(u => u.UsuEstado != 'I')
                .ToListAsync();

        public async Task AgregarAsync(UsuarioEntity usuario)
            => await _context.Usuarios.AddAsync(usuario);

        public void Actualizar(UsuarioEntity usuario)
            => _context.Usuarios.Update(usuario);
    }

}

