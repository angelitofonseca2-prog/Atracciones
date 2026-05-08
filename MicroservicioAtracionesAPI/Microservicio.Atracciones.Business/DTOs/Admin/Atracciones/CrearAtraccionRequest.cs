using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microservicio.Atracciones.Business.DTOs.Admin.Atracciones
{
    public class CrearAtraccionRequest
    {
        [Required] public Guid DestinoGuid { get; set; }
        [MaxLength(30)] public string? NumEstablecimiento { get; set; }

        [Required][MaxLength(200)] public string Nombre { get; set; } = string.Empty;
        [MaxLength(2000)] public string? Descripcion { get; set; }
        [MaxLength(300)] public string? Direccion { get; set; }

        [Range(1, int.MaxValue)] public int? DuracionMinutos { get; set; }
        [MaxLength(300)] public string? PuntoEncuentro { get; set; }

        [Range(0, double.MaxValue)] public decimal? PrecioReferencia { get; set; }

        // GUIDs de las relaciones N:M a crear junto con la atracción
        [Required]
        [MinLength(1)]
        public IList<Guid> CategoriaGuids { get; set; } = new List<Guid>();

        [Required]
        [MinLength(1)]
        public IList<Guid> IdiomaGuids { get; set; } = new List<Guid>();

        [Required]
        [MinLength(1)]
        public IList<Guid> ImagenGuids { get; set; } = new List<Guid>();

        [Required]
        [MinLength(1)]
        public IList<Guid> IncluyeGuids { get; set; } = new List<Guid>();
    }
}
