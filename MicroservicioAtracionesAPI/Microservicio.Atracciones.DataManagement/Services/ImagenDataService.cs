using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microservicio.Atracciones.DataManagement.Interfaces;
using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using Microsoft.EntityFrameworkCore;

namespace Microservicio.Atracciones.DataManagement.Services
{
    public class ImagenDataService : IImagenDataService
    {
        private readonly AtraccionesDbContext _context;

        public ImagenDataService(AtraccionesDbContext context)
            => _context = context;

        public async Task<IReadOnlyList<ImagenDataModel>> ListarActivasAsync()
            => await _context.Imagenes.AsNoTracking()
                .Where(i => i.ImgEstado == 'A')
                .OrderByDescending(i => i.ImgFechaIngreso)
                .Select(ToModelExpression())
                .ToListAsync();

        public async Task<ImagenDataModel?> ObtenerPorGuidAsync(Guid imgGuid)
            => await _context.Imagenes.AsNoTracking()
                .Where(i => i.ImgGuid == imgGuid && i.ImgEstado == 'A')
                .Select(ToModelExpression())
                .FirstOrDefaultAsync();

        public async Task<ImagenDataModel?> ObtenerPorUrlAsync(string url)
        {
            var normalizada = url.Trim().ToUpper();
            return await _context.Imagenes.AsNoTracking()
                .Where(i => i.ImgEstado == 'A' && i.ImgUrl.ToUpper() == normalizada)
                .Select(ToModelExpression())
                .FirstOrDefaultAsync();
        }

        public async Task CrearAsync(ImagenDataModel model, string usuarioAccion, string ip)
        {
            var entity = new ImagenEntity
            {
                ImgGuid = Guid.NewGuid(),
                ImgUrl = model.ImgUrl,
                ImgDescripcion = model.ImgDescripcion,
                ImgEstado = 'A',
                ImgFechaIngreso = DateTime.UtcNow,
                ImgUsuarioIngreso = usuarioAccion,
                ImgIpIngreso = ip
            };

            await _context.Imagenes.AddAsync(entity);
            await _context.SaveChangesAsync();

            model.ImgId = entity.ImgId;
            model.ImgGuid = entity.ImgGuid;
            model.ImgEstado = entity.ImgEstado;
        }

        public async Task ActualizarAsync(ImagenDataModel model, string usuarioAccion, string ip)
        {
            var entity = await _context.Imagenes.FirstOrDefaultAsync(i => i.ImgId == model.ImgId && i.ImgEstado == 'A')
                ?? throw new InvalidOperationException($"Imagen {model.ImgId} no encontrada.");

            entity.ImgUrl = model.ImgUrl;
            entity.ImgDescripcion = model.ImgDescripcion;
            entity.ImgFechaMod = DateTime.UtcNow;
            entity.ImgUsuarioMod = usuarioAccion;
            entity.ImgIpMod = ip;
            await _context.SaveChangesAsync();
        }

        public async Task EliminarLogicoAsync(int imgId, string usuarioAccion, string ip)
        {
            var entity = await _context.Imagenes.FirstOrDefaultAsync(i => i.ImgId == imgId && i.ImgEstado == 'A')
                ?? throw new InvalidOperationException($"Imagen {imgId} no encontrada.");

            entity.ImgEstado = 'I';
            entity.ImgFechaEliminacion = DateTime.UtcNow;
            entity.ImgUsuarioEliminacion = usuarioAccion;
            entity.ImgIpEliminacion = ip;
            await _context.SaveChangesAsync();
        }

        private static System.Linq.Expressions.Expression<Func<ImagenEntity, ImagenDataModel>> ToModelExpression()
            => i => new ImagenDataModel
            {
                ImgId = i.ImgId,
                ImgGuid = i.ImgGuid,
                ImgUrl = i.ImgUrl,
                ImgDescripcion = i.ImgDescripcion,
                ImgEstado = i.ImgEstado,
                ImgFechaIngreso = i.ImgFechaIngreso
            };
    }
}
