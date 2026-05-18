using Atracciones.MsAtracciones.DataManagement.Models;

namespace Atracciones.MsAtracciones.DataManagement.Interfaces;

public interface IInventarioRepository
{
    Task<PagedResult<AtraccionIndexRow>> ListarConFiltrosAsync(AtraccionFiltroQuery filtro, CancellationToken ct = default);
    Task<AtraccionDetalleRow?> ObtenerDetalleAsync(Guid atGuid, CancellationToken ct = default);
    Task<IReadOnlyList<TicketRow>> ListarTicketsPorAtraccionAsync(Guid atGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioRow>> ListarHorariosDisponiblesPorAtraccionAsync(Guid atGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioProximoRow>> ListarHorariosPorTicketGuidAsync(Guid tckGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioProximoRow>> ListarHorariosPorAtraccionVentanaAsync(Guid atGuid, int diasAdelante, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioProximoRow>> ListarHorariosPorAtraccionPublicAsync(
        Guid atGuid,
        int diasAdelante,
        bool soloDisponibles,
        CancellationToken ct = default);
    Task<HorarioTicketPublicoRow?> ObtenerHorarioConTicketPorAtraccionAsync(
        Guid atGuid,
        Guid horGuid,
        CancellationToken ct = default);

    Task<IReadOnlyList<AtraccionFiltroComputationRow>> ListarActivasParaFiltrosAsync(int maxItems, CancellationToken ct = default);

    Task<AtraccionAdminRow?> ObtenerAtraccionAdminAsync(Guid atGuid, CancellationToken ct = default);
    Task<AtraccionAdminCompletaRow?> ObtenerAtraccionAdminCompletaAsync(Guid atGuid, CancellationToken ct = default);
    Task<PagedResult<AtraccionAdminRow>> ListarAtraccionesAdminAsync(AtraccionAdminFiltroQuery filtro, CancellationToken ct = default);
    Task<Guid> CrearAtraccionConRelacionesAsync(AtraccionPersistModel model, CancellationToken ct = default);
    Task ActualizarAtraccionConRelacionesAsync(AtraccionPersistModel model, CancellationToken ct = default);
    Task EliminarAtraccionLogicoAsync(Guid atGuid, string usuario, string ip, CancellationToken ct = default);

    Task<TicketAdminRow?> ObtenerTicketAdminAsync(Guid tckGuid, CancellationToken ct = default);
    Task<IReadOnlyList<TicketAdminRow>> ListarTicketsAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TicketAdminRow>> ListarTicketsPorAtraccionAdminAsync(Guid atGuid, CancellationToken ct = default);
    Task<Guid> CrearTicketAsync(TicketPersistModel model, CancellationToken ct = default);
    Task ActualizarTicketAsync(TicketPersistModel model, CancellationToken ct = default);
    Task EliminarTicketLogicoAsync(Guid tckGuid, string usuario, string ip, CancellationToken ct = default);

    Task<HorarioAdminRow?> ObtenerHorarioAdminAsync(Guid horGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioAdminRow>> ListarHorariosAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<HorarioAdminRow>> ListarHorariosPorTicketAdminAsync(Guid tckGuid, CancellationToken ct = default);
    Task<IReadOnlyList<HorarioAdminRow>> ListarHorariosPorAtraccionAdminAsync(Guid atGuid, CancellationToken ct = default);
    Task<Guid> CrearHorarioAsync(HorarioPersistModel model, CancellationToken ct = default);
    Task ActualizarHorarioAsync(HorarioPersistModel model, CancellationToken ct = default);
    Task EliminarHorarioLogicoAsync(Guid horGuid, string usuario, string ip, CancellationToken ct = default);

    Task<int?> DescontarCuposHorarioAsync(Guid horGuid, int cantidad, CancellationToken ct = default);
    Task<int?> IncrementarCuposHorarioAsync(Guid horGuid, int cantidad, CancellationToken ct = default);

    Task<(decimal Precio, string TipoParticipante, Guid AtGuid)?> ObtenerPrecioTicketActivoAsync(Guid tckGuid, CancellationToken ct = default);

    Task<(string AtNombre, DateOnly HorFecha, DateOnly HorFechaFin, TimeOnly HorHoraInicio, TimeOnly? HorHoraFin, Guid TckGuid)?> ObtenerHorarioReservaSnapshotAsync(
        Guid horGuid,
        Guid atGuidEsperado,
        CancellationToken ct = default);

    Task<IReadOnlyList<AtraccionFiltroSeedRow>> ListarSemillasFiltroAsync(int maxItems, CancellationToken ct = default);
    Task<bool> ExisteAtraccionActivaAsync(Guid atGuid, CancellationToken ct = default);
}
