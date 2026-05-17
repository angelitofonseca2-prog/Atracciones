using Atracciones.MsOrquestador.DataAccess.Context;
using Atracciones.MsOrquestador.DataAccess.Entities;
using Atracciones.MsOrquestador.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Atracciones.MsOrquestador.DataAccess.Repositories;

public sealed class PayPalPaymentRepository : IPayPalPaymentRepository
{
    private readonly OrquestadorDbContext _db;

    public PayPalPaymentRepository(OrquestadorDbContext db) => _db = db;

    public async Task<long> InsertAsync(
        Guid revGuid,
        string paypalOrderId,
        string estado,
        decimal monto,
        string moneda,
        string? checkoutPayloadJson,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var entity = new PayPalPaymentEntity
        {
            RevGuid = revGuid,
            PaypalOrderId = paypalOrderId,
            EstadoPago = estado,
            MontoEsperado = monto,
            Moneda = moneda,
            CheckoutPayloadJson = checkoutPayloadJson,
            CreatedUtc = now,
            UpdatedUtc = now,
        };
        _db.PayPalPayments.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.PayPaymentId;
    }

    public async Task<PayPalPaymentRow?> GetByPaypalOrderIdAsync(string paypalOrderId, CancellationToken ct = default)
    {
        var x = await _db.PayPalPayments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.PaypalOrderId == paypalOrderId, ct);
        return x is null ? null : Map(x);
    }

    public async Task<PayPalPaymentRow?> GetByPaypalCaptureIdAsync(string paypalCaptureId, CancellationToken ct = default)
    {
        var x = await _db.PayPalPayments.AsNoTracking()
            .FirstOrDefaultAsync(e => e.PaypalCaptureId == paypalCaptureId, ct);
        return x is null ? null : Map(x);
    }

    public async Task UpdateEstadoAsync(
        long payPaymentId,
        string estado,
        string? captureId,
        string? chargebackStatus,
        CancellationToken ct = default)
    {
        var row = await _db.PayPalPayments.FirstOrDefaultAsync(x => x.PayPaymentId == payPaymentId, ct);
        if (row is null)
            return;

        row.EstadoPago = estado;
        row.UpdatedUtc = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(captureId))
            row.PaypalCaptureId = captureId;

        if (chargebackStatus is not null)
            row.ChargebackStatus = chargebackStatus;

        await _db.SaveChangesAsync(ct);
    }

    public async Task ClearCheckoutPayloadAsync(long payPaymentId, CancellationToken ct = default)
    {
        var row = await _db.PayPalPayments.FirstOrDefaultAsync(x => x.PayPaymentId == payPaymentId, ct);
        if (row is null)
            return;

        row.CheckoutPayloadJson = null;
        row.UpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private static PayPalPaymentRow Map(PayPalPaymentEntity x) =>
        new(x.PayPaymentId, x.RevGuid, x.PaypalOrderId, x.PaypalCaptureId,
            x.EstadoPago, x.MontoEsperado, x.Moneda, x.ChargebackStatus, x.CheckoutPayloadJson);
}
