using Microservicio.Atracciones.DataManagement.Models.Clientes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IClienteDataService
    {
        Task<ClienteDataModel?> ObtenerPorIdAsync(int cliId);
        Task<ClienteDataModel?> ObtenerPorGuidAsync(Guid cliGuid);
        Task<ClienteDataModel?> ObtenerPorUsuarioIdAsync(int usuId);
        Task<ClienteDataModel?> ObtenerPorNumeroIdentificacionAsync(string numero);
        Task<IReadOnlyList<ClienteDataModel>> ListarAsync();
        Task CrearAsync(ClienteDataModel model);
        Task ActualizarAsync(ClienteDataModel model);
        Task EliminarLogicoAsync(int cliId, string usuarioAccion, string ip);
        Task<bool> ExisteIdentificacionAsync(string numero);
    }
}
