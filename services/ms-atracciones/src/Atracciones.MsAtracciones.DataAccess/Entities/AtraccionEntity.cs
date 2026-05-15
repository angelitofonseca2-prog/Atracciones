namespace Atracciones.MsAtracciones.DataAccess.Entities;

public sealed class AtraccionEntity
{
    public Guid AtGuid { get; set; }
    public Guid DesGuid { get; set; }
    public string DesNombreSnap { get; set; } = string.Empty;
    public string DesPaisSnap { get; set; } = string.Empty;
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
    public bool AtDisponible { get; set; } = true;

    public DateTime AtFechaIngreso { get; set; }
    public string AtUsuarioIngreso { get; set; } = string.Empty;
    public string AtIpIngreso { get; set; } = string.Empty;
    public DateTime? AtFechaMod { get; set; }
    public string? AtUsuarioMod { get; set; }
    public string? AtIpMod { get; set; }
    public DateTime? AtFechaEliminacion { get; set; }
    public string? AtUsuarioEliminacion { get; set; }
    public string? AtIpEliminacion { get; set; }
    public char AtEstado { get; set; } = 'A';

    public ICollection<TicketEntity> Tickets { get; set; } = new List<TicketEntity>();
    public ICollection<AtraccionCategoriaEntity> Categorias { get; set; } = new List<AtraccionCategoriaEntity>();
    public ICollection<AtraccionIdiomaEntity> Idiomas { get; set; } = new List<AtraccionIdiomaEntity>();
    public ICollection<AtraccionImagenEntity> Imagenes { get; set; } = new List<AtraccionImagenEntity>();
    public ICollection<AtraccionIncluyeEntity> Incluyes { get; set; } = new List<AtraccionIncluyeEntity>();
    public ICollection<ReseniaEntity> Resenias { get; set; } = new List<ReseniaEntity>();
}
