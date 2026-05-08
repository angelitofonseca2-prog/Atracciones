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
    public class CategoriaDataService : ICategoriaDataService
    {
        private readonly AtraccionesDbContext _context;
        public CategoriaDataService(AtraccionesDbContext context) => _context = context;

        public async Task<CategoriaDataModel?> ObtenerPorGuidAsync(Guid catGuid)
        {
            return await _context.Categorias.AsNoTracking()
                .Where(c => c.CatGuid == catGuid && c.CatEstado == 'A')
                .Include(c => c.Parent)
                .Select(c => new CategoriaDataModel
                {
                    CatId = c.CatId,
                    CatGuid = c.CatGuid,
                    CatNombre = c.CatNombre,
                    CatParentId = c.CatParentId,
                    CatEstado = c.CatEstado,
                    ParentGuid = c.Parent != null ? c.Parent.CatGuid : null,
                    ParentNombre = c.Parent != null ? c.Parent.CatNombre : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<CategoriaDataModel>> ListarActivasAsync()
        {
            return await _context.Categorias.AsNoTracking()
                .Where(c => c.CatEstado == 'A')
                .Include(c => c.Parent)
                .Select(c => new CategoriaDataModel
                {
                    CatId = c.CatId,
                    CatGuid = c.CatGuid,
                    CatNombre = c.CatNombre,
                    CatParentId = c.CatParentId,
                    CatEstado = c.CatEstado,
                    ParentGuid = c.Parent != null ? c.Parent.CatGuid : null,
                    ParentNombre = c.Parent != null ? c.Parent.CatNombre : null
                }).ToListAsync();
        }

        public async Task CrearAsync(CategoriaDataModel model, string usuarioAccion, string ip)
        {
            int? parentId = null;
            if (model.ParentGuid.HasValue)
            {
                parentId = await _context.Categorias
                    .Where(c => c.CatGuid == model.ParentGuid.Value && c.CatEstado == 'A')
                    .Select(c => (int?)c.CatId)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException($"Categoría padre {model.ParentGuid} no encontrada.");
            }

            var entity = new CategoriaEntity
            {
                CatGuid = Guid.NewGuid(),
                CatNombre = model.CatNombre,
                CatParentId = parentId,
                CatEstado = 'A',
                CatFechaIngreso = DateTime.UtcNow,
                CatUsuarioIngreso = usuarioAccion,
                CatIpIngreso = ip
            };

            await _context.Categorias.AddAsync(entity);
            await _context.SaveChangesAsync();

            model.CatId = entity.CatId;
            model.CatGuid = entity.CatGuid;
            model.CatParentId = entity.CatParentId;
            model.CatEstado = entity.CatEstado;
        }

        public async Task ActualizarAsync(CategoriaDataModel model, string usuarioAccion, string ip)
        {
            var entity = await _context.Categorias.FirstOrDefaultAsync(c => c.CatId == model.CatId && c.CatEstado == 'A')
                ?? throw new InvalidOperationException($"Categoría {model.CatId} no encontrada.");

            int? parentId = null;
            if (model.ParentGuid.HasValue)
            {
                parentId = await _context.Categorias
                    .Where(c => c.CatGuid == model.ParentGuid.Value && c.CatEstado == 'A' && c.CatId != model.CatId)
                    .Select(c => (int?)c.CatId)
                    .FirstOrDefaultAsync()
                    ?? throw new InvalidOperationException($"Categoría padre {model.ParentGuid} no encontrada.");
            }

            entity.CatNombre = model.CatNombre;
            entity.CatParentId = parentId;
            entity.CatFechaMod = DateTime.UtcNow;
            entity.CatUsuarioMod = usuarioAccion;
            entity.CatIpMod = ip;

            await _context.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int catId, string usuarioAccion, string ip)
        {
            var entity = await _context.Categorias.FirstOrDefaultAsync(c => c.CatId == catId && c.CatEstado == 'A')
                ?? throw new InvalidOperationException($"Categoría {catId} no encontrada.");

            entity.CatEstado = 'I';
            entity.CatFechaEliminacion = DateTime.UtcNow;
            entity.CatUsuarioEliminacion = usuarioAccion;
            entity.CatIpEliminacion = ip;
            await _context.SaveChangesAsync();
        }
    }
}
