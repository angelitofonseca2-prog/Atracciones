using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Resenias
{
    public class ReseniaAdminResponse
    {
        public string RsnGuid { get; set; } = string.Empty;
        public string AtraccionNombre { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public short Rating { get; set; }
        public string? Comentario { get; set; }
        public char Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

}
