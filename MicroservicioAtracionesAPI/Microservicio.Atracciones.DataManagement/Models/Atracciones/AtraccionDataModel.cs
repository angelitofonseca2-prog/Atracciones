using Microservicio.Atracciones.DataManagement.Models.Catalogos;

namespace Microservicio.Atracciones.DataManagement.Models.Atracciones
{
    public class AtraccionDataModel
    {
        public int AtId { get; set; }
        public Guid AtGuid { get; set; }
        public int DesId { get; set; }
        public string? AtNumEstablecimiento { get; set; }
        public string AtNombre { get; set; } = string.Empty;
        public string? AtDescripcion { get; set; }
        public int AtTotalResenias { get; set; }
        public string? AtDireccion { get; set; }
        public int? AtDuracionMinutos { get; set; }
        public string? AtPuntoEncuentro { get; set; }
        public decimal? AtPrecioReferencia { get; set; }
        public bool AtIncluyeAcompaniante { get; set; }
        public bool AtIncluyeTransporte { get; set; }
        public bool AtDisponible { get; set; }
        public char AtEstado { get; set; }

        // Auditoría
        public DateTime AtFechaIngreso { get; set; }
        public string AtUsuarioIngreso { get; set; } = string.Empty;
        public string AtIpIngreso { get; set; } = string.Empty;
        public DateTime? AtFechaMod { get; set; }
        public string? AtUsuarioMod { get; set; }
        public string? AtIpMod { get; set; }
        public DateTime? AtFechaEliminacion { get; set; }
        public string? AtUsuarioEliminacion { get; set; }
        public string? AtIpEliminacion { get; set; }

        // Destino (desnormalizado para evitar joins en Business)
        public Guid DesGuid { get; set; }
        public string DesNombre { get; set; } = string.Empty;
        public string DesPais { get; set; } = string.Empty;

        // Relaciones N:M resueltas como listas planas
        public IReadOnlyList<CategoriaDataModel> Categorias { get; set; } = new List<CategoriaDataModel>();
        public IReadOnlyList<IdiomaDataModel> Idiomas { get; set; } = new List<IdiomaDataModel>();
        public IReadOnlyList<ImagenDataModel> Imagenes { get; set; } = new List<ImagenDataModel>();
        public IReadOnlyList<IncluyeDataModel> Incluyes { get; set; } = new List<IncluyeDataModel>();

        // Precio mínimo calculado desde TICKET (para listado)
        public decimal? PrecioDesde { get; set; }

        // Calificación promedio (para listado)
        public double? CalificacionPromedio { get; set; }
    }
}
