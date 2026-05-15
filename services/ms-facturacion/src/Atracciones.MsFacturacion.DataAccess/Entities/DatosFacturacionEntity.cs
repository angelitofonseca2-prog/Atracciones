namespace Atracciones.MsFacturacion.DataAccess.Entities;

public sealed class DatosFacturacionEntity
{
    public Guid DfacGuid { get; set; }
    public Guid FacGuid { get; set; }
    public string DfacNombre { get; set; } = string.Empty;
    public string DfacCorreo { get; set; } = string.Empty;
    public string? DfacTelefono { get; set; }

    public FacturaEntity Factura { get; set; } = null!;
}
