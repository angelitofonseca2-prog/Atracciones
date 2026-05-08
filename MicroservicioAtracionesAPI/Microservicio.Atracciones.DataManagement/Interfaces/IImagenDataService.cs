using Microservicio.Atracciones.DataManagement.Models.Catalogos;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IImagenDataService
    {
        Task<IReadOnlyList<ImagenDataModel>> ListarActivasAsync();
        Task<ImagenDataModel?> ObtenerPorGuidAsync(Guid imgGuid);
        Task<ImagenDataModel?> ObtenerPorUrlAsync(string url);
        Task CrearAsync(ImagenDataModel model, string usuarioAccion, string ip);
        Task ActualizarAsync(ImagenDataModel model, string usuarioAccion, string ip);
        Task EliminarLogicoAsync(int imgId, string usuarioAccion, string ip);
    }
}
