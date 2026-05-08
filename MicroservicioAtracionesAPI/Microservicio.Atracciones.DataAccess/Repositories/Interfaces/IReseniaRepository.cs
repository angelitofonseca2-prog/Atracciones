using Microservicio.Atracciones.DataAccess.Entities.Reservas;

namespace Microservicio.Atracciones.DataAccess.Repositories.Interfaces
{
    public interface IReseniaRepository
    {
        Task<ReseniaEntity?> ObtenerPorIdAsync(int rsnId);
        Task<ReseniaEntity?> ObtenerPorGuidAsync(Guid rsnGuid);
        Task<ReseniaEntity?> ObtenerPorReservaAsync(int revId);
        Task<bool> ExistePorClienteYAtraccionAsync(int cliId, int atId);
        Task<IReadOnlyList<ReseniaEntity>> ListarPorAtraccionAsync(int atId);
        Task AgregarAsync(ReseniaEntity resenia);
        void Actualizar(ReseniaEntity resenia);
    }
}
