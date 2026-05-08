using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataManagement.Models.Reservas
{
    public class ReservaDetalleDataModel
    {
        public int RdetId { get; set; }
        public Guid RdetGuid { get; set; }
        public int RevId { get; set; }
        public int TckId { get; set; }
        public int RdetCantidad { get; set; }
        public decimal RdetPrecioUnit { get; set; }  // snapshot del precio
        public decimal RdetSubtotal { get; set; }
        public char RdetEstado { get; set; }

        // Auditoría
        public DateTime RdetFechaIngreso { get; set; }
        public string RdetUsuarioIngreso { get; set; } = string.Empty;
        public string RdetIpIngreso { get; set; } = string.Empty;
        public DateTime? RdetFechaEliminacion { get; set; }
        public string? RdetUsuarioEliminacion { get; set; }
        public string? RdetIpEliminacion { get; set; }

        // Nombre del tipo de ticket (desnormalizado para respuesta)
        public string TckTipoParticipante { get; set; } = string.Empty;
    }
}
