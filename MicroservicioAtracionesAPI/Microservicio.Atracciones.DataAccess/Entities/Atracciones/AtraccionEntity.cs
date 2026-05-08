using Microservicio.Atracciones.DataAccess.Entities.Catalogos;
using Microservicio.Atracciones.DataAccess.Entities.Reservas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microservicio.Atracciones.DataAccess.Entities.Atracciones
{
    public class AtraccionEntity
    {
        public int AtId { get; set; }
        public Guid AtGuid { get; set; }
        public int DesId { get; set; }
        public string? AtNumEstablecimiento { get; set; }
        public string AtNombre { get; set; } = string.Empty;
        public string? AtDescripcion { get; set; }
        public int AtTotalResenias { get; set; } = 0;
        public string? AtDireccion { get; set; }
        public int? AtDuracionMinutos { get; set; }
        public string? AtPuntoEncuentro { get; set; }
        public decimal? AtPrecioReferencia { get; set; }
        public bool AtIncluyeAcompaniante { get; set; } = false;
        public bool AtIncluyeTransporte { get; set; } = false;
        public bool AtDisponible { get; set; } = true;

        // Auditoría ingreso
        public DateTime AtFechaIngreso { get; set; }
        public string AtUsuarioIngreso { get; set; } = string.Empty;
        public string AtIpIngreso { get; set; } = string.Empty;

        // Auditoría modificación
        public DateTime? AtFechaMod { get; set; }
        public string? AtUsuarioMod { get; set; }
        public string? AtIpMod { get; set; }

        // Auditoría eliminación lógica
        public DateTime? AtFechaEliminacion { get; set; }
        public string? AtUsuarioEliminacion { get; set; }
        public string? AtIpEliminacion { get; set; }

        public char AtEstado { get; set; } = 'A';

        // ----------------------------------------------------------------
        //  Navegación
        // ----------------------------------------------------------------
        public DestinoEntity Destino { get; set; } = null!;
        public ICollection<CategoriaAtraccionEntity> CategoriasAtracciones { get; set; } = new List<CategoriaAtraccionEntity>();
        public ICollection<IdiomaAtraccionEntity> IdiomasAtracciones { get; set; } = new List<IdiomaAtraccionEntity>();
        public ICollection<ImagenAtraccionEntity> ImagenesAtracciones { get; set; } = new List<ImagenAtraccionEntity>();
        public ICollection<AtraccionIncluyeEntity> AtraccionesIncluyen { get; set; } = new List<AtraccionIncluyeEntity>();
        public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();
        public ICollection<ReseniaEntity> Resenias { get; set; } = new List<ReseniaEntity>();
    }
}
