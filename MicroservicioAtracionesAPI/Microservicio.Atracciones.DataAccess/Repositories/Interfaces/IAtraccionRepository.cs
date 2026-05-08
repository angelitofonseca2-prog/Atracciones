using Microservicio.Atracciones.DataAccess.Entities.Atracciones;

namespace Microservicio.Atracciones.DataAccess.Repositories.Interfaces
{
    public interface IAtraccionRepository
    {
        Task<AtraccionEntity?> ObtenerPorIdAsync(int atId);
        Task<AtraccionEntity?> ObtenerPorGuidAsync(Guid atGuid);
        Task<IReadOnlyList<AtraccionEntity>> ListarActivasAsync();
        Task AgregarAsync(AtraccionEntity atraccion);
        void Actualizar(AtraccionEntity atraccion);

        // Operaciones sobre tablas pivote (manejadas dentro del agregado Atraccion)
        Task AgregarCategoriaAsync(CategoriaAtraccionEntity categoriaAtraccion);
        Task AgregarIdiomaAsync(IdiomaAtraccionEntity idiomaAtraccion);
        Task AgregarImagenAsync(ImagenAtraccionEntity imagenAtraccion);
        Task AgregarIncluyeAsync(AtraccionIncluyeEntity atraccionIncluye);
        void EliminarCategoria(CategoriaAtraccionEntity categoriaAtraccion);
        void EliminarIdioma(IdiomaAtraccionEntity idiomaAtraccion);
        void EliminarImagen(ImagenAtraccionEntity imagenAtraccion);
        void EliminarIncluye(AtraccionIncluyeEntity atraccionIncluye);
    }
}
