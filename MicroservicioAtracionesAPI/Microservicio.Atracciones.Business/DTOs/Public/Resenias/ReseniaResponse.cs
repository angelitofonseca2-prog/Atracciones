using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Public.Resenias
{
    public class ReseniaResponse
    {
        public string RsnGuid { get; set; } = string.Empty;
        public string AtraccionNombre { get; set; } = string.Empty;
        public short Rating { get; set; }
        public string? Comentario { get; set; }
        public string FechaCreacion { get; set; } = string.Empty;
    }
}
