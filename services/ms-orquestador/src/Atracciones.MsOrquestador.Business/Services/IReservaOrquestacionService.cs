using Atracciones.MsOrquestador.Business.Models;

namespace Atracciones.MsOrquestador.Business.Services;

public interface IReservaOrquestacionService
{
    Task<ReservaResponseDto> CrearReservaAsync(
        CrearReservaOrquestadorDto request,
        Guid? usuGuid,
        string? authorizationBearer,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default);

    /// <summary>Tras evidencia de pago externa verificada (p. ej. captura PayPal). Si <paramref name="compensarSiFallaFactura"/> es false, no se anula la reserva si falla la factura.</summary>
    Task<FacturaStubResponseDto> CompletarPagoReservaYFacturaAsync(
        Guid revGuid,
        ConfirmarPagoOrquestadorDto request,
        string usuarioAccion,
        string ip,
        string correlationId,
        bool compensarSiFallaFactura,
        CancellationToken ct = default);

    Task CancelarReservaAsync(
        Guid revGuid,
        string motivo,
        Guid usuGuidCliente,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default);

    Task<ReservaResponseDto> ObtenerReservaAsync(
        Guid revGuid,
        Guid usuGuidCliente,
        CancellationToken ct = default);

    Task<(IReadOnlyList<ReservaResponseDto> Items, int Total)> ListarMisReservasAsync(
        Guid usuGuidCliente,
        int page,
        int limit,
        CancellationToken ct = default);
}
