using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Reservas
{
    public class ReservaDetalleAdminResponse
    {
        public string TipoParticipante { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnit { get; set; }
        public decimal Subtotal { get; set; }
    }
}
