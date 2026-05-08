using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Atracciones
{
    public class AtraccionAdminResponse
    {
        public string AtGuid { get; set; } = string.Empty;
        public string DestinoGuid { get; set; } = string.Empty;
        public string? NumEstablecimiento { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Direccion { get; set; }
        public int? DuracionMinutos { get; set; }
        public string? PuntoEncuentro { get; set; }
        public decimal? PrecioReferencia { get; set; }
        public bool Disponible { get; set; }
        public char Estado { get; set; }
        public int TotalResenias { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string? ImagenPrincipal { get; set; }
        public IList<string> CategoriaGuids { get; set; } = new List<string>();
        public IList<string> IdiomaGuids { get; set; } = new List<string>();
        public IList<string> ImagenGuids { get; set; } = new List<string>();
        public IList<string> IncluyeGuids { get; set; } = new List<string>();
        public IList<string> Idiomas { get; set; } = new List<string>();
        public IList<string> Categorias { get; set; } = new List<string>();
        public IList<string> Imagenes { get; set; } = new List<string>();
        public IList<string> Incluyes { get; set; } = new List<string>();
    }
}
