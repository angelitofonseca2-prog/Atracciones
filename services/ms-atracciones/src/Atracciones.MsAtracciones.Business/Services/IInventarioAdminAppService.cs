using Atracciones.MsAtracciones.Business.Common;
using Atracciones.MsAtracciones.Business.Dtos.Admin.Atracciones;

namespace Atracciones.MsAtracciones.Business.Services;

public interface IInventarioAdminAppService
{
    Task<DataPagedResult<AtraccionAdminResponse>> ListarAsync(AtraccionAdminFiltroRequest filtro, CancellationToken ct = default);
    Task<AtraccionAdminResponse> ObtenerPorGuidAsync(Guid atGuid, CancellationToken ct = default);
    Task<AtraccionAdminResponse> CrearAsync(CrearAtraccionRequest request, string usuario, string ip, CancellationToken ct = default);
    Task<AtraccionAdminResponse> ActualizarAsync(Guid atGuid, ActualizarAtraccionRequest request, string usuario, string ip, CancellationToken ct = default);
    Task EliminarAsync(Guid atGuid, string usuario, string ip, CancellationToken ct = default);
}
