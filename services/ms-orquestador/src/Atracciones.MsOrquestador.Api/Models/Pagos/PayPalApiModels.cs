using Atracciones.MsOrquestador.Api.Models.Reservas;

namespace Atracciones.MsOrquestador.Api.Models.Pagos;

public sealed class CrearPayPalOrderApiRequest
{
    /// <summary>Reserva a materializar tras captura PayPal (flujo actual).</summary>
    public CrearReservaApiRequest? Reserva { get; set; }
    /// <summary>Solo reservas pendientes legacy (estado P) creadas antes del cambio.</summary>
    public Guid RevGuid { get; set; }
    public string? RevCodigo { get; set; }
}

public sealed class CapturarPayPalOrderApiRequest
{
    public Guid RevGuid { get; set; }
    public string? RevCodigo { get; set; }
    public string PaypalOrderId { get; set; } = string.Empty;
    public string NombreReceptor { get; set; } = string.Empty;
    public string? ApellidoReceptor { get; set; }
    public string CorreoReceptor { get; set; } = string.Empty;
    public string? TelefonoReceptor { get; set; }
    public string? Observacion { get; set; }
}
