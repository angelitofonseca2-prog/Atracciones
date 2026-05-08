using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IIncluyeDataService
    {
        Task<IncluyeDataModel?> ObtenerPorGuidAsync(Guid incGuid);
        Task<IReadOnlyList<IncluyeDataModel>> ListarActivosAsync();
        Task CrearAsync(IncluyeDataModel model);
        Task ActualizarAsync(IncluyeDataModel model);
        Task EliminarLogicoAsync(int incId);
    }
}
