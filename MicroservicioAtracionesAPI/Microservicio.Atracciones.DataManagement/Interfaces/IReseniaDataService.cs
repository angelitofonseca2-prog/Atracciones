using Microservicio.Atracciones.DataManagement.Models.Reservas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IReseniaDataService
    {
        Task<ReseniaDataModel?> ObtenerPorIdAsync(int rsnId);
        Task<ReseniaDataModel?> ObtenerPorGuidAsync(Guid rsnGuid);
        Task<ReseniaDataModel?> ObtenerPorReservaAsync(int revId);
        Task<IReadOnlyList<ReseniaDataModel>> ListarPorAtraccionAsync(int atId);
        Task CrearAsync(ReseniaDataModel model);
        Task ActualizarAsync(ReseniaDataModel model);
        Task EliminarLogicoAsync(int rsnId, string usuarioAccion, string ip);
        Task<bool> YaTieneReseniaAsync(int revId);
        Task<bool> YaTieneReseniaParaAtraccionAsync(int cliId, int atId);
    }
}
