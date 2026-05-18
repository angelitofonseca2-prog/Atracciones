namespace Atracciones.MsOrquestador.Business.Models;

public sealed class ClienteInvitadoOrquestadorDto
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

public sealed class CrearReservaOrquestadorDto
{
    public Guid AtGuid { get; set; }
    public Guid HorGuid { get; set; }
    /// <summary>Día de visita (yyyy-MM-dd) cuando el horario tiene rango de fechas.</summary>
    public string? FechaVisita { get; set; }
    public IList<LineaTicketOrquestadorDto> Lineas { get; set; } = new List<LineaTicketOrquestadorDto>();
    public string? OrigenCanal { get; set; }
    public ClienteInvitadoOrquestadorDto? ClienteInvitado { get; set; }
}

public sealed class LineaTicketOrquestadorDto
{
    public Guid TckGuid { get; set; }
    public int Cantidad { get; set; }
}

public sealed class ConfirmarPagoOrquestadorDto
{
    public string NombreReceptor { get; set; } = string.Empty;
    public string? ApellidoReceptor { get; set; }
    public string CorreoReceptor { get; set; } = string.Empty;
    public string? TelefonoReceptor { get; set; }
    public string? Observacion { get; set; }
}

public sealed class ReservaDetalleResponseDto
{
    public string TckTipoParticipante { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnit { get; set; }
    public decimal Subtotal { get; set; }
}

public sealed class ReservaResponseDto
{
    public string RevGuid { get; set; } = string.Empty;
    public string AtGuid { get; set; } = string.Empty;
    public string RevCodigo { get; set; } = string.Empty;
    public string HorFecha { get; set; } = string.Empty;
    public string HorHoraInicio { get; set; } = string.Empty;
    public string? HorHoraFin { get; set; }
    public string AtraccionNombre { get; set; } = string.Empty;
    public decimal RevSubtotal { get; set; }
    public decimal RevValorIva { get; set; }
    public decimal RevTotal { get; set; }
    public string Moneda { get; set; } = "USD";
    public string RevEstado { get; set; } = string.Empty;
    public DateTime RevFechaReservaUtc { get; set; }
    public IList<ReservaDetalleResponseDto> Detalle { get; set; } = new List<ReservaDetalleResponseDto>();
    public Dictionary<string, string?> Links { get; set; } = new();
}

public sealed class FacturaStubResponseDto
{
    public string RevGuid { get; set; } = string.Empty;
    public string FacGuid { get; set; } = string.Empty;
    public string FacNumero { get; set; } = string.Empty;
    public string RevCodigo { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "USD";
    public DateTime FechaEmision { get; set; }
    public string Estado { get; set; } = "P";
    public string NombreReceptor { get; set; } = string.Empty;
    public string CorreoReceptor { get; set; } = string.Empty;
}
