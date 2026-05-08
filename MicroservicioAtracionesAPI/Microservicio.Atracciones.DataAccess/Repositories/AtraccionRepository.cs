using Microsoft.EntityFrameworkCore;
using Microservicio.Atracciones.DataAccess.Context;
using Microservicio.Atracciones.DataAccess.Entities.Atracciones;
using Microservicio.Atracciones.DataAccess.Repositories.Interfaces;

namespace Microservicio.Atracciones.DataAccess.Repositories
{
    public class AtraccionRepository : IAtraccionRepository
    {
        private readonly AtraccionesDbContext _context;
        public AtraccionRepository(AtraccionesDbContext context) => _context = context;

        public async Task<AtraccionEntity?> ObtenerPorIdAsync(int atId)
            => await _context.Atracciones
                .Include(x => x.Destino)
                .Include(x => x.CategoriasAtracciones).ThenInclude(ca => ca.Categoria)
                .Include(x => x.IdiomasAtracciones).ThenInclude(ia => ia.Idioma)
                .Include(x => x.ImagenesAtracciones).ThenInclude(ima => ima.Imagen)
                .Include(x => x.AtraccionesIncluyen).ThenInclude(ai => ai.Incluye)
                .FirstOrDefaultAsync(x => x.AtId == atId && x.AtEstado == 'A');

        public async Task<AtraccionEntity?> ObtenerPorGuidAsync(Guid atGuid)
            => await _context.Atracciones
                .Include(x => x.Destino)
                .Include(x => x.CategoriasAtracciones).ThenInclude(ca => ca.Categoria)
                .Include(x => x.IdiomasAtracciones).ThenInclude(ia => ia.Idioma)
                .Include(x => x.ImagenesAtracciones).ThenInclude(ima => ima.Imagen)
                .Include(x => x.AtraccionesIncluyen).ThenInclude(ai => ai.Incluye)
                .FirstOrDefaultAsync(x => x.AtGuid == atGuid && x.AtEstado == 'A');

        public async Task<IReadOnlyList<AtraccionEntity>> ListarActivasAsync()
            => await _context.Atracciones
                .AsNoTracking()
                .Include(x => x.Destino)
                .Where(x => x.AtEstado == 'A')
                .OrderBy(x => x.AtNombre)
                .ToListAsync();

        public async Task AgregarAsync(AtraccionEntity atraccion)
            => await _context.Atracciones.AddAsync(atraccion);

        public void Actualizar(AtraccionEntity atraccion)
            => _context.Atracciones.Update(atraccion);

        public async Task AgregarCategoriaAsync(CategoriaAtraccionEntity categoriaAtraccion)
            => await _context.CategoriasAtracciones.AddAsync(categoriaAtraccion);

        public async Task AgregarIdiomaAsync(IdiomaAtraccionEntity idiomaAtraccion)
            => await _context.IdiomasAtracciones.AddAsync(idiomaAtraccion);

        public async Task AgregarImagenAsync(ImagenAtraccionEntity imagenAtraccion)
            => await _context.ImagenesAtracciones.AddAsync(imagenAtraccion);

        public async Task AgregarIncluyeAsync(AtraccionIncluyeEntity atraccionIncluye)
            => await _context.AtraccionesIncluyen.AddAsync(atraccionIncluye);

        public void EliminarCategoria(CategoriaAtraccionEntity categoriaAtraccion)
            => _context.CategoriasAtracciones.Remove(categoriaAtraccion);

        public void EliminarIdioma(IdiomaAtraccionEntity idiomaAtraccion)
            => _context.IdiomasAtracciones.Remove(idiomaAtraccion);

        public void EliminarImagen(ImagenAtraccionEntity imagenAtraccion)
            => _context.ImagenesAtracciones.Remove(imagenAtraccion);

        public void EliminarIncluye(AtraccionIncluyeEntity atraccionIncluye)
            => _context.AtraccionesIncluyen.Remove(atraccionIncluye);
    }
}
