using Microservicio.Atracciones.DataAccess.Entities.Clientes;
using Microservicio.Atracciones.DataAccess.Entities.Facturacion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Reservas
{
    public class ReservaEntity
    {
        public int RevId { get; set; }
        public Guid RevGuid { get; set; }
        public string RevCodigo { get; set; } = string.Empty;
        public int CliId { get; set; }
        public int HorId { get; set; }
        public DateTime RevFechaReservaUtc { get; set; }

        // Totales financieros
        public decimal RevSubtotal { get; set; }
        public decimal RevValorIva { get; set; }
        public decimal RevTotal { get; set; }
        public string? RevOrigenCanal { get; set; }

        // Auditoría ingreso
        public string RevUsuarioIngreso { get; set; } = string.Empty;
        public string RevIpIngreso { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? RevFechaMod { get; set; }
        public string? RevUsuarioMod { get; set; }
        public string? RevIpMod { get; set; }

        // Auditoría cancelación
        public DateTime? RevFechaCancelacion { get; set; }
        public string? RevUsuarioCancelacion { get; set; }
        public string? RevIpCancelacion { get; set; }
        public string? RevMotivoCancelacion { get; set; }

        // Estado: 'A' = Activa, 'I' = Inactiva, 'C' = Cancelada
        public char RevEstado { get; set; } = 'A';

        // Navegación
        public ClienteEntity Cliente { get; set; } = null!;
        public HorarioEntity Horario { get; set; } = null!;
        public ICollection<ReservaDetalleEntity> ReservasDetalle { get; set; } = new List<ReservaDetalleEntity>();
        public ReseniaEntity? Resenia { get; set; }
        public FacturaEntity? Factura { get; set; }
    }
}
