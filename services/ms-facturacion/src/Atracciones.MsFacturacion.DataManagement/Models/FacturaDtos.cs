namespace Atracciones.MsFacturacion.DataManagement.Models;

public sealed class FacturaEmitidaDto
{
    public Guid FacGuid { get; set; }
    public string FacNumero { get; set; } = string.Empty;
    public Guid RevGuid { get; set; }
    public Guid CliGuid { get; set; }
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "USD";
    public DateTime FechaEmisionUtc { get; set; }
    public char Estado { get; set; } = 'A';
    public string NombreReceptor { get; set; } = string.Empty;
    public string CorreoReceptor { get; set; } = string.Empty;
    public string RevCodigoSnap { get; set; } = string.Empty;
}

public sealed class EmitirFacturaInternaDto
{
    public Guid RevGuid { get; set; }
    public Guid CliGuid { get; set; }
    public string NombreReceptor { get; set; } = string.Empty;
    public string CorreoReceptor { get; set; } = string.Empty;
    public string TelefonoReceptor { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "USD";
    public string RevCodigoSnap { get; set; } = string.Empty;
    public string UsuarioEmision { get; set; } = string.Empty;
    public string IpEmision { get; set; } = string.Empty;
}

public sealed class FacturaAdminRowDto
{
    public Guid FacGuid { get; set; }
    public string FacNumero { get; set; } = string.Empty;
    public Guid RevGuid { get; set; }
    public Guid CliGuid { get; set; }
    public string RevCodigoSnap { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Moneda { get; set; } = "USD";
    public DateTime FechaEmisionUtc { get; set; }
    public char Estado { get; set; }
}
