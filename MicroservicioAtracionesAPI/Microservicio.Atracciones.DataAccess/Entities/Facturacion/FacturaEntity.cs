using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Facturacion
{
    public class FacturaEntity
    {
        public int FacId { get; set; }
        public Guid FacGuid { get; set; }
        public int RevId { get; set; }
        public string FacNumero { get; set; } = string.Empty;
        public DateTime FacFechaEmision { get; set; }
        public decimal FacTotal { get; set; }
        public string? FacObservacion { get; set; }
        public string? FacOrigenCanal { get; set; }

        // Auditoría ingreso
        public string FacUsuarioIngreso { get; set; } = string.Empty;
        public string FacIpIngreso { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? FacFechaMod { get; set; }
        public string? FacUsuarioMod { get; set; }
        public string? FacIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? FacFechaEliminacion { get; set; }
        public string? FacUsuarioEliminacion { get; set; }
        public string? FacIpEliminacion { get; set; }

        // Estado y control
        public char FacEstado { get; set; } = 'A';
        public string? FacMotivoInhabilitacion { get; set; }

        // Navegación
        public ReservaEntity Reserva { get; set; } = null!;
        public DatosFacturacionEntity? DatosFacturacion { get; set; }
    }
}
