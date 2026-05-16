namespace Atracciones.MsAtracciones.Business.Services;

public interface IInventarioCupoAppService
{
    Task<(bool Ok, string Mensaje, double Precio, string TipoParticipante, string AtGuid)> GetTicketPrecioAsync(
        Guid tckGuid,
        CancellationToken ct = default);

    Task<(bool Ok, string Mensaje, string AtraccionNombre, string HorFecha, string HorHoraInicio, string HorHoraFin, string TckGuid)> ObtenerHorarioParaReservaAsync(
        Guid horGuid,
        Guid atGuid,
        CancellationToken ct = default);

    Task<(bool Ok, string Mensaje, int Cupos)> ValidarYReservarAsync(Guid horGuid, int cantidad, CancellationToken ct = default);
    Task<(bool Ok, string Mensaje, int Cupos)> LiberarAsync(Guid horGuid, int cantidad, CancellationToken ct = default);
}
