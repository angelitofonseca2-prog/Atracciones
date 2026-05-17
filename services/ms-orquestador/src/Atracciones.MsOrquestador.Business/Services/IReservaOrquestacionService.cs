using Atracciones.MsOrquestador.Business.Models;

namespace Atracciones.MsOrquestador.Business.Services;

public interface IReservaOrquestacionService
{
    /// <summary>Valida precios y cliente; persiste intención para PayPal (sin cupo ni reserva en BD de ventas).</summary>
    Task<(PayPalCheckoutPayload Payload, decimal Total, string Moneda)> PrepararCheckoutPayPalAsync(
        CrearReservaOrquestadorDto request,
        Guid? usuGuid,
        string? authorizationBearer,
        string usuarioAccion,
        string ip,
        CancellationToken ct = default);

    /// <summary>Valida precios y disponibilidad de horario sin reservar cupo ni persistir reserva.</summary>
    Task<ReservaResponseDto> CotizarReservaAsync(
        CrearReservaOrquestadorDto request,
        Guid? usuGuid,
        string? authorizationBearer,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default);

    Task<ReservaResponseDto> CrearReservaAsync(
        CrearReservaOrquestadorDto request,
        Guid? usuGuid,
        string? authorizationBearer,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default);

    /// <summary>Tras captura PayPal: reserva cupo, crea reserva confirmada y emite factura.</summary>
    Task<FacturaStubResponseDto> MaterializarReservaTrasPagoCapturadoAsync(
        PayPalCheckoutPayload checkout,
        ConfirmarPagoOrquestadorDto facturacion,
        decimal montoCapturado,
        string monedaCapturada,
        string usuarioAccion,
        string ip,
        string correlationId,
        CancellationToken ct = default);

    /// <summary>Reserva pendiente legacy (estado P). Si <paramref name="compensarSiFallaFactura"/> es false, no se anula la reserva si falla la factura.</summary>
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
