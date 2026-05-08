using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Destinos
{
    public class DestinoResponse
    {
        public string DesGuid { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public char Estado { get; set; }
    }

}
