namespace Atracciones.MsOrquestador.DataManagement.Interfaces;

public sealed record PayPalPaymentRow(
    long PayPaymentId,
    Guid RevGuid,
    string PaypalOrderId,
    string? PaypalCaptureId,
    string EstadoPago,
    decimal MontoEsperado,
    string Moneda,
    string? ChargebackStatus);

public interface IPayPalPaymentRepository
{
    Task<long> InsertAsync(Guid revGuid, string paypalOrderId, string estado, decimal monto, string moneda, CancellationToken ct = default);
    Task<PayPalPaymentRow?> GetByPaypalOrderIdAsync(string paypalOrderId, CancellationToken ct = default);
    Task<PayPalPaymentRow?> GetByPaypalCaptureIdAsync(string paypalCaptureId, CancellationToken ct = default);
    Task UpdateEstadoAsync(long payPaymentId, string estado, string? captureId, string? chargebackStatus, CancellationToken ct = default);
}

