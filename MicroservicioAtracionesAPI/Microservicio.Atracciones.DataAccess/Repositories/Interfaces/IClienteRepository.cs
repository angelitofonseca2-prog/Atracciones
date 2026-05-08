using Microservicio.Atracciones.DataAccess.Entities.Clientes;

namespace Microservicio.Atracciones.DataAccess.Repositories.Interfaces
{
    public interface IClienteRepository
    {
        Task<ClienteEntity?> ObtenerPorIdAsync(int cliId);
        Task<ClienteEntity?> ObtenerPorGuidAsync(Guid cliGuid);
        Task<ClienteEntity?> ObtenerPorUsuarioIdAsync(int usuId);
        Task<ClienteEntity?> ObtenerPorNumeroIdentificacionAsync(string numeroIdentificacion);
        Task<IReadOnlyList<ClienteEntity>> ListarAsync();
        Task AgregarAsync(ClienteEntity cliente);
        void Actualizar(ClienteEntity cliente);
    }
}
