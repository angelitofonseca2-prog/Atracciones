using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Reservas
{
    public class ReservaDetalleEntity
    {
        public int RdetId { get; set; }
        public Guid RdetGuid { get; set; }
        public int RevId { get; set; }
        public int TckId { get; set; }

        // Precio congelado al momento de la reserva
        public int RdetCantidad { get; set; }
        public decimal RdetPrecioUnit { get; set; }  // snapshot de TckPrecio
        public decimal RdetSubtotal { get; set; }  // RdetCantidad × RdetPrecioUnit

        // Auditoría ingreso
        public DateTime RdetFechaIngreso { get; set; }
        public string RdetUsuarioIngreso { get; set; } = string.Empty;
        public string RdetIpIngreso { get; set; } = string.Empty;

        // Auditoría eliminación lógica
        public DateTime? RdetFechaEliminacion { get; set; }
        public string? RdetUsuarioEliminacion { get; set; }
        public string? RdetIpEliminacion { get; set; }

        public char RdetEstado { get; set; } = 'A';

        // Navegación
        public ReservaEntity Reserva { get; set; } = null!;
        public TicketEntity Ticket { get; set; } = null!;
    }
}
