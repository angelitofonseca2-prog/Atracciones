using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Atracciones
{
    public class AtraccionAdminFiltroRequest
    {
        public string? Ciudad { get; set; }
        public string? TextoBusqueda { get; set; }
        public char? Estado { get; set; }
        [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
        [Range(1, 50)] public int Limit { get; set; } = 10;
    }
}
