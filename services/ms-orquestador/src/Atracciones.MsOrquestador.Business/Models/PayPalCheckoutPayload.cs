namespace Atracciones.MsOrquestador.Business.Models;

/// <summary>Intención de reserva persistida al crear la orden PayPal; se materializa tras captura exitosa.</summary>
public sealed class PayPalCheckoutPayload
{
    public Guid RevGuid { get; set; }
    public Guid CliGuid { get; set; }
    public CrearReservaOrquestadorDto Reserva { get; set; } = null!;
}
