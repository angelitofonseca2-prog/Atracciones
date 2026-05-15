using Atracciones.MsReservas.DataManagement.Models;

namespace Atracciones.MsReservas.DataManagement.Interfaces;

public interface IReservaRepository
{
    Task<ReservaDetalladaDto> CrearPendienteAsync(CrearReservaInternaDto dto, CancellationToken ct = default);

    Task<ReservaDetalladaDto?> ObtenerPorGuidAsync(Guid revGuid, CancellationToken ct = default);

    Task<(IReadOnlyList<ReservaDetalladaDto> Items, int Total)> ListarPorClienteAsync(
        Guid cliGuid,
        int page,
        int limit,
        CancellationToken ct = default);

    Task<(IReadOnlyList<ReservaAdminRowDto> Items, int Total)> ListarAdminAsync(
        int page,
        int limit,
        char? estado,
        CancellationToken ct = default);

    Task<ReservaDetalladaDto?> ConfirmarPagadaAsync(Guid revGuid, string usuario, string ip, CancellationToken ct = default);

    Task<bool> AnularAsync(Guid revGuid, string motivo, string usuario, string ip, CancellationToken ct = default);
}
