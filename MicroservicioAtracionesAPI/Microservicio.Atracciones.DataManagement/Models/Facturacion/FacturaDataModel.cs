using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Facturacion
{
    public class FacturaDataModel
    {
        public int FacId { get; set; }
        public Guid FacGuid { get; set; }
        public int RevId { get; set; }
        public string FacNumero { get; set; } = string.Empty;
        public DateTime FacFechaEmision { get; set; }
        public decimal FacTotal { get; set; }
        public string? FacObservacion { get; set; }
        public string? FacOrigenCanal { get; set; }
        public char FacEstado { get; set; }
        public string? FacMotivoInhabilitacion { get; set; }

        // Auditoría
        public string FacUsuarioIngreso { get; set; } = string.Empty;
        public string FacIpIngreso { get; set; } = string.Empty;
        public DateTime? FacFechaMod { get; set; }
        public string? FacUsuarioMod { get; set; }
        public string? FacIpMod { get; set; }
        public DateTime? FacFechaEliminacion { get; set; }
        public string? FacUsuarioEliminacion { get; set; }
        public string? FacIpEliminacion { get; set; }

        // Datos del receptor
        public DatosFacturacionDataModel? DatosFacturacion { get; set; }
    }
}
