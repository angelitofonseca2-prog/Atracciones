using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class IdiomaDataService : IIdiomaDataService
    {
        private readonly AtraccionesDbContext _context;
        public IdiomaDataService(AtraccionesDbContext context) => _context = context;

        public async Task<IdiomaDataModel?> ObtenerPorGuidAsync(Guid idGuid)
        {
            return await _context.Idiomas.AsNoTracking()
                .Where(c => c.IdGuid == idGuid && c.IdEstado == 'A')
                .Select(c => new IdiomaDataModel
                {
                    IdId = c.IdId,
                    IdGuid = c.IdGuid,
                    IdDescripcion = c.IdDescripcion,
                    IdEstado = c.IdEstado
                }).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<IdiomaDataModel>> ListarActivosAsync()
        {
            return await _context.Idiomas.AsNoTracking()
                .Where(c => c.IdEstado == 'A')
                .Select(c => new IdiomaDataModel
                {
                    IdId = c.IdId,
                    IdGuid = c.IdGuid,
                    IdDescripcion = c.IdDescripcion,
                    IdEstado = c.IdEstado
                }).ToListAsync();
        }

        public async Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null)
        {
            var normalizada = descripcion.Trim().ToUpper();
            return await _context.Idiomas.AsNoTracking()
                .AnyAsync(i =>
                    i.IdDescripcion.ToUpper() == normalizada &&
                    (!excluirId.HasValue || i.IdId != excluirId.Value));
        }

        public async Task CrearAsync(IdiomaDataModel model, string usuarioAccion, string ip)
        {
            var entity = new IdiomaEntity
            {
                IdGuid = Guid.NewGuid(),
                IdDescripcion = model.IdDescripcion,
                IdEstado = 'A',
                IdFechaIngreso = DateTime.UtcNow,
                IdUsuarioIngreso = usuarioAccion,
                IdIpIngreso = ip
            };

            await _context.Idiomas.AddAsync(entity);
            await _context.SaveChangesAsync();

            model.IdId = entity.IdId;
            model.IdGuid = entity.IdGuid;
            model.IdEstado = entity.IdEstado;
        }

        public async Task ActualizarAsync(IdiomaDataModel model, string usuarioAccion, string ip)
        {
            var entity = await _context.Idiomas.FirstOrDefaultAsync(c => c.IdId == model.IdId && c.IdEstado == 'A')
                ?? throw new InvalidOperationException($"Idioma {model.IdId} no encontrado.");

            entity.IdDescripcion = model.IdDescripcion;
            entity.IdFechaMod = DateTime.UtcNow;
            entity.IdUsuarioMod = usuarioAccion;
            entity.IdIpMod = ip;
            await _context.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int idId, string usuarioAccion, string ip)
        {
            var entity = await _context.Idiomas.FirstOrDefaultAsync(c => c.IdId == idId && c.IdEstado == 'A')
                ?? throw new InvalidOperationException($"Idioma {idId} no encontrado.");

            entity.IdEstado = 'I';
            entity.IdFechaEliminacion = DateTime.UtcNow;
            entity.IdUsuarioEliminacion = usuarioAccion;
            entity.IdIpEliminacion = ip;
            await _context.SaveChangesAsync();
        }
    }
}
