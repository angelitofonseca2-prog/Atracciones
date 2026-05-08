using Microservicio.Atracciones.DataAccess.Entities.Catalogos;

namespace Microservicio.Atracciones.DataAccess.Repositories.Interfaces
{
    public interface IDestinoRepository
    {
        Task<DestinoEntity?> ObtenerPorIdAsync(int desId);
        Task<DestinoEntity?> ObtenerPorGuidAsync(Guid desGuid);
        Task<DestinoEntity?> ObtenerPorNombreAsync(string nombre);
        Task<IReadOnlyList<DestinoEntity>> ListarActivosAsync();
        Task AgregarAsync(DestinoEntity destino);
        void Actualizar(DestinoEntity destino);
    }
}
