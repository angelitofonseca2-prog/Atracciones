using Atracciones.MsAtracciones.DataManagement.Interfaces;

namespace Atracciones.MsAtracciones.Business.Services;

public sealed class InventarioCupoAppService : IInventarioCupoAppService
{
    private readonly IInventarioRepository _repo;

    public InventarioCupoAppService(IInventarioRepository repo) => _repo = repo;

    public async Task<(bool Ok, string Mensaje, double Precio, string TipoParticipante, string AtGuid)> GetTicketPrecioAsync(
        Guid tckGuid,
        CancellationToken ct = default)
    {
        var row = await _repo.ObtenerPrecioTicketActivoAsync(tckGuid, ct);
        if (row is null)
            return (false, "Ticket no encontrado o inactivo.", 0, "", "");

        return (true, string.Empty, (double)row.Value.Precio, row.Value.TipoParticipante, row.Value.AtGuid.ToString());
    }

    public async Task<(bool Ok, string Mensaje, string AtraccionNombre, string HorFecha, string HorHoraInicio, string HorHoraFin)> ObtenerHorarioParaReservaAsync(
        Guid horGuid,
        Guid atGuid,
        CancellationToken ct = default)
    {
        var row = await _repo.ObtenerHorarioReservaSnapshotAsync(horGuid, atGuid, ct);
        if (row is null)
            return (false, "Horario no disponible o no coincide con la atracción.", "", "", "", "");

        var fecha = row.Value.HorFecha.ToString("yyyy-MM-dd");
        var ini = row.Value.HorHoraInicio.ToString("HH:mm");
        var fin = row.Value.HorHoraFin?.ToString("HH:mm") ?? "";
        return (true, string.Empty, row.Value.AtNombre, fecha, ini, fin);
    }

    public async Task<(bool Ok, string Mensaje, int Cupos)> ValidarYReservarAsync(Guid horGuid, int cantidad, CancellationToken ct = default)
    {
        if (cantidad <= 0)
            return (false, "cantidad_personas debe ser mayor a 0.", 0);

        var cupos = await _repo.DescontarCuposHorarioAsync(horGuid, cantidad, ct);
        if (cupos is null)
            return (false, "Sin cupos suficientes u horario no disponible.", 0);

        return (true, string.Empty, cupos.Value);
    }

    public async Task<(bool Ok, string Mensaje, int Cupos)> LiberarAsync(Guid horGuid, int cantidad, CancellationToken ct = default)
    {
        if (cantidad <= 0)
            return (false, "cantidad_personas debe ser mayor a 0.", 0);

        var cupos = await _repo.IncrementarCuposHorarioAsync(horGuid, cantidad, ct);
        if (cupos is null)
            return (false, "Horario no encontrado.", 0);

        return (true, string.Empty, cupos.Value);
    }
}
