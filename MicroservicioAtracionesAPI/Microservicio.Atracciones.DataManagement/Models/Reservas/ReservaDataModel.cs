using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Reservas
{
    public class ReservaDataModel
    {
        public int RevId { get; set; }
        public Guid RevGuid { get; set; }
        public string RevCodigo { get; set; } = string.Empty;
        public int CliId { get; set; }
        public int HorId { get; set; }
        public DateTime RevFechaReservaUtc { get; set; }
        public decimal RevSubtotal { get; set; }
        public decimal RevValorIva { get; set; }
        public decimal RevTotal { get; set; }
        public string? RevOrigenCanal { get; set; }
        public char RevEstado { get; set; }

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

        public string ClienteNombre { get; set; } = string.Empty;
        public string AtraccionNombre { get; set; } = string.Empty;
        public DateOnly? HorFecha { get; set; }
        public TimeOnly? HorHoraInicio { get; set; }

        // Líneas de detalle
        public IReadOnlyList<ReservaDetalleDataModel> Detalle { get; set; } = new List<ReservaDetalleDataModel>();
    }
}
