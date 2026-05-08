using Microservicio.Atracciones.DataManagement.Models.Catalogos;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IDestinoDataService
    {
        Task<DestinoDataModel?> ObtenerPorIdAsync(int desId);
        Task<DestinoDataModel?> ObtenerPorGuidAsync(Guid desGuid);
        Task<DestinoDataModel?> ObtenerPorNombreAsync(string nombre);
        Task<IReadOnlyList<DestinoDataModel>> ListarActivosAsync();
        Task CrearAsync(DestinoDataModel model);
        Task ActualizarAsync(DestinoDataModel model);
        Task EliminarLogicoAsync(int desId, string usuarioAccion, string ip);
    }
}
