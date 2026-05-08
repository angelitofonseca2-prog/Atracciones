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
    public class DestinoRepository : IDestinoRepository
    {
        private readonly AtraccionesDbContext _context;
        public DestinoRepository(AtraccionesDbContext context) => _context = context;

        public async Task<DestinoEntity?> ObtenerPorIdAsync(int desId)
            => await _context.Destinos.FirstOrDefaultAsync(x => x.DesId == desId && x.DesEstado == 'A');

        public async Task<DestinoEntity?> ObtenerPorGuidAsync(Guid desGuid)
            => await _context.Destinos.FirstOrDefaultAsync(x => x.DesGuid == desGuid && x.DesEstado == 'A');

        public async Task<DestinoEntity?> ObtenerPorNombreAsync(string nombre)
            => await _context.Destinos
                .AsNoTracking()
                .FirstOrDefaultAsync(d => 
                    EF.Functions.ILike(d.DesNombre, nombre.Trim()) && 
                    d.DesEstado == 'A');

        public async Task<IReadOnlyList<DestinoEntity>> ListarActivosAsync()
            => await _context.Destinos
                .AsNoTracking()
                .Where(x => x.DesEstado == 'A')
                .OrderBy(x => x.DesNombre)
                .ToListAsync();

        public async Task AgregarAsync(DestinoEntity destino)
            => await _context.Destinos.AddAsync(destino);

        public void Actualizar(DestinoEntity destino)
            => _context.Destinos.Update(destino);
    }
}
