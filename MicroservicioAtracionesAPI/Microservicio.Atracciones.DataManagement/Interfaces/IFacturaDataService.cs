using Microservicio.Atracciones.DataManagement.Models.Common;
using Microservicio.Atracciones.DataManagement.Models.Facturacion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Interfaces
{
    public interface IFacturaDataService
    {
        Task<FacturaDataModel?> ObtenerPorIdAsync(int facId);
        Task<FacturaDataModel?> ObtenerPorGuidAsync(Guid facGuid);
        Task<FacturaDataModel?> ObtenerPorReservaAsync(int revId);
        Task<DataPagedResult<FacturaDataModel>> ListarAdminAsync(int page, int limit, char? estado = null);
        Task<DataPagedResult<FacturaDataModel>> ListarPorClienteAsync(int cliId, int page, int limit);
        Task CrearAsync(FacturaDataModel model);
        Task InhabilitarAsync(int facId, string motivo, string usuarioAccion, string ip);
        Task<bool> ExisteNumeroFacturaAsync(string numero);
        Task<string> GenerarNumeroSecuencialAsync(int anio);
    }
}
