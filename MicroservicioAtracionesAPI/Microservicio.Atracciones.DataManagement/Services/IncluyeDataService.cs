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
    public class IncluyeDataService : IIncluyeDataService
    {
        private readonly AtraccionesDbContext _context;
        public IncluyeDataService(AtraccionesDbContext context) => _context = context;

        public async Task<IncluyeDataModel?> ObtenerPorGuidAsync(Guid incGuid)
        {
            return await _context.Incluyes.AsNoTracking()
                .Where(c => c.IncGuid == incGuid && c.IncEstado == 'A')
                .Select(c => new IncluyeDataModel
                {
                    IncId = c.IncId,
                    IncGuid = c.IncGuid,
                    IncDescripcion = c.IncDescripcion,
                    IncEstado = c.IncEstado
                }).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<IncluyeDataModel>> ListarActivosAsync()
        {
            return await _context.Incluyes.AsNoTracking()
                .Where(c => c.IncEstado == 'A')
                .Select(c => new IncluyeDataModel
                {
                    IncId = c.IncId,
                    IncGuid = c.IncGuid,
                    IncDescripcion = c.IncDescripcion,
                    IncEstado = c.IncEstado
                }).ToListAsync();
        }

        public async Task CrearAsync(IncluyeDataModel model)
        {
            var entity = new IncluyeEntity
            {
                IncGuid = Guid.NewGuid(),
                IncDescripcion = model.IncDescripcion,
                IncEstado = 'A'
            };

            await _context.Incluyes.AddAsync(entity);
            await _context.SaveChangesAsync();

            model.IncId = entity.IncId;
            model.IncGuid = entity.IncGuid;
            model.IncEstado = entity.IncEstado;
        }

        public async Task ActualizarAsync(IncluyeDataModel model)
        {
            var entity = await _context.Incluyes.FirstOrDefaultAsync(c => c.IncId == model.IncId && c.IncEstado == 'A')
                ?? throw new InvalidOperationException($"Incluye {model.IncId} no encontrado.");

            entity.IncDescripcion = model.IncDescripcion;
            await _context.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int incId)
        {
            var entity = await _context.Incluyes.FirstOrDefaultAsync(c => c.IncId == incId && c.IncEstado == 'A')
                ?? throw new InvalidOperationException($"Incluye {incId} no encontrado.");

            entity.IncEstado = 'I';
            await _context.SaveChangesAsync();
        }
    }
}
