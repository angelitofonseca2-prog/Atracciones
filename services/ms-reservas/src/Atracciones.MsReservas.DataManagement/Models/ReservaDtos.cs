namespace Atracciones.MsReservas.DataManagement.Models;

public sealed class CrearReservaInternaDto
{
    public Guid? RevGuidPreasignado { get; init; }
    public Guid CliGuid { get; init; }
    public Guid AtGuid { get; init; }
    public Guid HorGuid { get; init; }
    public string RevCodigo { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal ValorIva { get; init; }
    public decimal Total { get; init; }
    public string? OrigenCanal { get; init; }
    public string UsuarioIngreso { get; init; } = string.Empty;
    public string IpIngreso { get; init; } = string.Empty;
    public string AtraccionNombreSnap { get; init; } = string.Empty;
    public string HorFechaSnap { get; init; } = string.Empty;
    public string HorHoraInicioSnap { get; init; } = string.Empty;
    public string HorHoraFinSnap { get; init; } = string.Empty;
    public IReadOnlyList<CrearReservaLineaInternaDto> Lineas { get; init; } = Array.Empty<CrearReservaLineaInternaDto>();
}

public sealed class CrearReservaLineaInternaDto
{
    public Guid TckGuid { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnit { get; init; }
    public decimal SubtotalLinea { get; init; }
    public string TipoParticipante { get; init; } = string.Empty;
}

public sealed class ReservaDetalladaDto
{
    public Guid RevGuid { get; init; }
    public string RevCodigo { get; init; } = string.Empty;
    public Guid CliGuid { get; init; }
    public Guid AtGuid { get; init; }
    public Guid HorGuid { get; init; }
    public char Estado { get; init; }
    public decimal Subtotal { get; init; }
    public decimal ValorIva { get; init; }
    public decimal Total { get; init; }
    public string Moneda { get; init; } = "USD";
    public string? OrigenCanal { get; init; }
    public DateTime RevFechaReservaUtc { get; init; }
    public string AtraccionNombreSnap { get; init; } = string.Empty;
    public string HorFechaSnap { get; init; } = string.Empty;
    public string HorHoraInicioSnap { get; init; } = string.Empty;
    public string HorHoraFinSnap { get; init; } = string.Empty;
    public IReadOnlyList<ReservaDetalleDto> Detalle { get; init; } = Array.Empty<ReservaDetalleDto>();
}

public sealed class ReservaDetalleDto
{
    public Guid TckGuid { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnit { get; init; }
    public decimal SubtotalLinea { get; init; }
    public string TipoParticipante { get; init; } = string.Empty;
}

public sealed class ReservaAdminRowDto
{
    public Guid RevGuid { get; init; }
    public string RevCodigo { get; init; } = string.Empty;
    public Guid CliGuid { get; init; }
    public char Estado { get; init; }
    public decimal Total { get; init; }
    public DateTime FechaReserva { get; init; }
    public string AtraccionNombreSnap { get; init; } = string.Empty;
    public string HorFechaSnap { get; init; } = string.Empty;
    public string HorHoraInicioSnap { get; init; } = string.Empty;
}
