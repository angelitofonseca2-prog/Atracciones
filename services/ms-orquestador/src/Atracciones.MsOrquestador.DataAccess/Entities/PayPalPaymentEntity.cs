namespace Atracciones.MsOrquestador.DataAccess.Entities;

/// <summary>Estado de un intento de pago PayPal ligado a una reserva (esquema orq).</summary>
public sealed class PayPalPaymentEntity
{
    public long PayPaymentId { get; set; }
    public Guid RevGuid { get; set; }
    /// <summary>ID de orden PayPal (v2/checkout/orders).</summary>
    public string PaypalOrderId { get; set; } = string.Empty;
    /// <summary>ID de captura tras éxito; nulo hasta capturar.</summary>
    public string? PaypalCaptureId { get; set; }
    /// <summary>ORDER_CREATED, APPROVED, CAPTURED, FAILED, VOIDED, DISPUTED, EN_REVISION.</summary>
    public string EstadoPago { get; set; } = string.Empty;
    public decimal MontoEsperado { get; set; }
    public string Moneda { get; set; } = "USD";
    public string? ChargebackStatus { get; set; }
    /// <summary>JSON de <see cref="Business.Models.PayPalCheckoutPayload"/> hasta completar la captura.</summary>
    public string? CheckoutPayloadJson { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

