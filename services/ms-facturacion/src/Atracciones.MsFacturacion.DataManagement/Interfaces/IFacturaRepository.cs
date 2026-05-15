using Atracciones.MsFacturacion.DataManagement.Models;

namespace Atracciones.MsFacturacion.DataManagement.Interfaces;

public interface IFacturaRepository
{
    Task<FacturaEmitidaDto> EmitirAsync(EmitirFacturaInternaDto dto, CancellationToken ct = default);

    Task<FacturaEmitidaDto?> ObtenerPorGuidAsync(Guid facGuid, CancellationToken ct = default);

    Task<(IReadOnlyList<FacturaEmitidaDto> Items, int Total)> ListarPorClienteAsync(
        Guid cliGuid,
        int page,
        int limit,
        CancellationToken ct = default);

    Task<(IReadOnlyList<FacturaAdminRowDto> Items, int Total)> ListarAdminAsync(
        int page,
        int limit,
        CancellationToken ct = default);
}
