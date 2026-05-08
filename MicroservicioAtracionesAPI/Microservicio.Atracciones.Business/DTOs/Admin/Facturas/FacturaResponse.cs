using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Facturas
{
    public class FacturaResponse
    {
        public string FacGuid { get; set; } = string.Empty;
        public string FacNumero { get; set; } = string.Empty;
        public string RevCodigo { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Moneda { get; set; } = "USD";
        public DateTime FechaEmision { get; set; }
        public char Estado { get; set; }
        public string NombreReceptor { get; set; } = string.Empty;
        public string CorreoReceptor { get; set; } = string.Empty;
    }
}
