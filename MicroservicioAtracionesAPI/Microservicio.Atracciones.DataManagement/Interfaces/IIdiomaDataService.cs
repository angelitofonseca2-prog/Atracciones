using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IIdiomaDataService
    {
        Task<IdiomaDataModel?> ObtenerPorGuidAsync(Guid idGuid);
        Task<IReadOnlyList<IdiomaDataModel>> ListarActivosAsync();
        Task<bool> ExisteDescripcionAsync(string descripcion, int? excluirId = null);
        Task CrearAsync(IdiomaDataModel model, string usuarioAccion, string ip);
        Task ActualizarAsync(IdiomaDataModel model, string usuarioAccion, string ip);
        Task EliminarLogicoAsync(int idId, string usuarioAccion, string ip);
    }
}
