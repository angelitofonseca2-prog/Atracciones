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

    public async Task<(bool Ok, string Mensaje, string AtraccionNombre, string HorFecha, string HorFechaFin, string HorHoraInicio, string HorHoraFin, string TckGuid)> ObtenerHorarioParaReservaAsync(
        Guid horGuid,
        Guid atGuid,
        DateOnly? fechaVisita,
        CancellationToken ct = default)
    {
        var row = await _repo.ObtenerHorarioReservaSnapshotAsync(horGuid, atGuid, ct);
        if (row is null)
            return (false, "Horario no disponible o no coincide con la atracción.", "", "", "", "", "", "");

        var inicio = row.Value.HorFecha;
        var fin = row.Value.HorFechaFin;
        var esRango = fin > inicio;

        DateOnly visita;
        if (esRango)
        {
            if (!fechaVisita.HasValue)
                return (false, "Debe indicar el día de visita dentro del rango del horario.", "", "", "", "", "", "");

            visita = fechaVisita.Value;
            if (visita < inicio || visita > fin)
                return (false, "La fecha de visita no está dentro del rango del horario.", "", "", "", "", "", "");
        }
        else
        {
            visita = inicio;
            if (fechaVisita.HasValue && fechaVisita.Value != inicio)
                return (false, "La fecha de visita no coincide con el horario seleccionado.", "", "", "", "", "", "");
        }

        var ini = row.Value.HorHoraInicio.ToString("HH:mm");
        var finHora = row.Value.HorHoraFin?.ToString("HH:mm") ?? "";
        return (
            true,
            string.Empty,
            row.Value.AtNombre,
            visita.ToString("yyyy-MM-dd"),
            fin.ToString("yyyy-MM-dd"),
            ini,
            finHora,
            row.Value.TckGuid.ToString("D"));
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
            return (false, "No se pudo liberar cupo.", 0);

        return (true, string.Empty, cupos.Value);
    }
}
