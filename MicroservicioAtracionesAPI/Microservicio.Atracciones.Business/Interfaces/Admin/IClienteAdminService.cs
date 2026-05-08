using Microservicio.Atracciones.Business.DTOs.Admin.Clientes;
using Microservicio.Atracciones.DataManagement.Models.Common;

namespace Microservicio.Atracciones.Business.Interfaces.Admin
{
    public interface IClienteAdminService
    {
        Task<ClienteResponse> CrearAsync(CrearClienteRequest request, string usuarioAccion, string ip);
        Task<ClienteResponse> ActualizarAsync(Guid cliGuid, ActualizarClienteRequest request, string usuarioAccion, string ip);
        Task EliminarAsync(Guid cliGuid, string usuarioAccion, string ip);
        Task<ClienteResponse> ObtenerPorGuidAsync(Guid cliGuid);
        Task<DataPagedResult<ClienteResponse>> ListarAsync(ClienteFiltroRequest filtro);
    }
}
