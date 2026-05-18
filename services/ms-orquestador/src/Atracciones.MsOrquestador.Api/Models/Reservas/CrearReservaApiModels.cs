using System.ComponentModel.DataAnnotations;

namespace Atracciones.MsOrquestador.Api.Models.Reservas;

public sealed class ClienteInvitadoApiRequest
{
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string NumeroIdentificacion { get; set; } = string.Empty;
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string? RazonSocial { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}

public sealed class ReservaDetalleApiRequest
{
    public Guid TckGuid { get; set; }
    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public sealed class CrearReservaApiRequest
{
    public Guid AtGuid { get; set; }
    public Guid HorGuid { get; set; }
    /// <summary>Día de visita (yyyy-MM-dd) si el horario abarca varias fechas.</summary>
    public string? FechaVisita { get; set; }
    [MinLength(1)]
    public IList<ReservaDetalleApiRequest> Lineas { get; set; } = new List<ReservaDetalleApiRequest>();
    public string? OrigenCanal { get; set; }
    public ClienteInvitadoApiRequest? ClienteInvitado { get; set; }
}

public sealed class ConfirmarPagoApiRequest
{
    public string NombreReceptor { get; set; } = string.Empty;
    public string? ApellidoReceptor { get; set; }
    public string CorreoReceptor { get; set; } = string.Empty;
    public string? TelefonoReceptor { get; set; }
    public string? Observacion { get; set; }
    /// <summary>Si el pago fue con PayPal, ID de la orden capturada en cliente.</summary>
    public string? PaypalOrderId { get; set; }
}

public sealed class CancelarReservaApiRequest
{
    public string? Motivo { get; set; }
}
