namespace Atracciones.MsFacturacion.DataAccess.Entities;

public sealed class FacturaEntity
{
    public Guid FacGuid { get; set; }
    public Guid RevGuid { get; set; }
    public Guid CliGuid { get; set; }
    public string FacNumero { get; set; } = string.Empty;
    public decimal FacTotal { get; set; }
    public string FacMoneda { get; set; } = "USD";
    public DateTime FacFechaEmisionUtc { get; set; }
    public char FacEstado { get; set; } = 'A';
    public string RevCodigoSnap { get; set; } = string.Empty;
    public string FacUsuarioIngreso { get; set; } = string.Empty;
    public string FacIpIngreso { get; set; } = string.Empty;

    public DatosFacturacionEntity? Datos { get; set; }
}
