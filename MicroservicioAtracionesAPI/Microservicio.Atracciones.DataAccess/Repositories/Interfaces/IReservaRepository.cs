using Microservicio.Atracciones.DataAccess.Entities.Reservas;

namespace Microservicio.Atracciones.DataAccess.Repositories.Interfaces
{
    public interface IReservaRepository
    {
        Task<ReservaEntity?> ObtenerPorIdAsync(int revId);
        Task<ReservaEntity?> ObtenerPorGuidAsync(Guid revGuid);
        Task<ReservaEntity?> ObtenerPorCodigoAsync(string revCodigo);
        Task<IReadOnlyList<ReservaEntity>> ListarPorClienteAsync(int cliId);
        Task AgregarAsync(ReservaEntity reserva);
        void Actualizar(ReservaEntity reserva);

        // Detalle (dentro del agregado Reserva)
        Task AgregarDetalleAsync(ReservaDetalleEntity detalle);
    }
}
