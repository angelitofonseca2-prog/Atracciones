using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Reservas
{
    public class ReservaAdminFiltroRequest
    {
        public Guid? CliGuid { get; set; }
        public Guid? AtGuid { get; set; }
        public char? Estado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
        [Range(1, 50)] public int Limit { get; set; } = 10;
    }
}
