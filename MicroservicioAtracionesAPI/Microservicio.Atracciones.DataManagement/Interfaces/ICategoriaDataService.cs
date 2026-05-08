using Microservicio.Atracciones.DataManagement.Models.Catalogos;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface ICategoriaDataService
    {
        Task<CategoriaDataModel?> ObtenerPorGuidAsync(Guid catGuid);
        Task<IReadOnlyList<CategoriaDataModel>> ListarActivasAsync();
        Task CrearAsync(CategoriaDataModel model, string usuarioAccion, string ip);
        Task ActualizarAsync(CategoriaDataModel model, string usuarioAccion, string ip);
        Task EliminarLogicoAsync(int catId, string usuarioAccion, string ip);
    }
}
