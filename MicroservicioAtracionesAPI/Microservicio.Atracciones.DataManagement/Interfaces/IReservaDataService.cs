using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Reservas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IReservaDataService
    {
        Task<ReservaDataModel?> ObtenerPorIdAsync(int revId);
        Task<ReservaDataModel?> ObtenerPorGuidAsync(Guid revGuid);
        Task<DataPagedResult<ReservaDataModel>> ListarPorClienteAsync(int cliId, int page, int limit);
        Task<DataPagedResult<ReservaDataModel>> ListarAdminAsync(ReservaFiltroDataModel filtro);
        Task CrearAsync(ReservaDataModel model);
        Task ActualizarEstadoAsync(int revId, char nuevoEstado, string motivo, string usuarioAccion, string ip);
        Task<bool> ExisteCodigoAsync(string codigo);
    }

}
