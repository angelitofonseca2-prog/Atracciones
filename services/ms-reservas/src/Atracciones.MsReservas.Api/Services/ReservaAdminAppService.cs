using Atracciones.Contracts.Inventario.V1;
using Atracciones.MsReservas.Api.Integration;
using Atracciones.MsReservas.Api.Models.Admin;
using Atracciones.MsReservas.DataManagement.Interfaces;
using Microsoft.Extensions.Logging;

namespace Atracciones.MsReservas.Api.Services;

public sealed class ReservaAdminAppService
{
    private readonly IReservaRepository _repo;
    private readonly AtraccionInventarioService.AtraccionInventarioServiceClient _inventario;
    private readonly ILogger<ReservaAdminAppService> _logger;

    public ReservaAdminAppService(
        IReservaRepository repo,
        AtraccionInventarioService.AtraccionInventarioServiceClient inventario,
        ILogger<ReservaAdminAppService> logger)
    {
        _repo = repo;
        _inventario = inventario;
        _logger = logger;
    }

    public async Task ActualizarEstadoAsync(
        Guid revGuid,
        ActualizarEstadoReservaRequest request,
        CancellationToken ct = default)
    {
        ValidarRequest(request);

        var reserva = await _repo.ObtenerPorGuidAsync(revGuid, ct)
            ?? throw new KeyNotFoundException("Reserva no existe.");

        var estadoActual = reserva.Estado;
        var nuevo = request.NuevoEstado;

        if (estadoActual == 'C')
            throw new InvalidOperationException("No se puede modificar el estado de una reserva cancelada.");
        if (estadoActual == 'I' && nuevo == 'A')
            throw new InvalidOperationException("No se puede reactivar una reserva inactivada directamente.");

        if (nuevo == 'A' && estadoActual == 'P')
        {
            await _repo.ConfirmarPagadaAsync(revGuid, "admin", "0.0.0.0", ct);
            return;
        }

        var liberarCupo = nuevo == 'C' && (estadoActual == 'P' || estadoActual == 'A');
        await _repo.ActualizarEstadoAsync(revGuid, nuevo, request.Motivo, "admin", "0.0.0.0", ct);

        if (liberarCupo)
            await LiberarCupoBestEffortAsync(reserva, ct);
    }

    public Task CancelarAsync(Guid revGuid, string motivo, CancellationToken ct = default) =>
        ActualizarEstadoAsync(revGuid, new ActualizarEstadoReservaRequest { NuevoEstado = 'C', Motivo = motivo }, ct);

    public Task AnularAsync(Guid revGuid, string motivo, CancellationToken ct = default) =>
        ActualizarEstadoAsync(revGuid, new ActualizarEstadoReservaRequest { NuevoEstado = 'I', Motivo = motivo }, ct);

    private async Task LiberarCupoBestEffortAsync(DataManagement.Models.ReservaDetalladaDto reserva, CancellationToken ct)
    {
        var totalPersonas = reserva.Detalle.Sum(d => d.Cantidad);
        if (totalPersonas <= 0)
            return;

        try
        {
            await _inventario.LiberarCupoAsync(new LiberarCupoRequest
            {
                HorGuid = reserva.HorGuid.ToString("D"),
                CantidadPersonas = totalPersonas,
                ReservaGuid = reserva.RevGuid.ToString("D"),
            }, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "No se pudo liberar cupo tras cancelación admin de la reserva {RevGuid}",
                reserva.RevGuid);
        }
    }

    private static void ValidarRequest(ActualizarEstadoReservaRequest request)
    {
        if (request.NuevoEstado is not ('A' or 'I' or 'C'))
            throw new ArgumentException("Estado inválido. Valores aceptados: A, I, C.");

        if (request.NuevoEstado == 'C' && string.IsNullOrWhiteSpace(request.Motivo))
            throw new ArgumentException("El motivo es obligatorio para cancelar una reserva.");
    }
}
