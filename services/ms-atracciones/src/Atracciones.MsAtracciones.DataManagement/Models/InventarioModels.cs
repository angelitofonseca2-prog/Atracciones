namespace Atracciones.MsAtracciones.DataManagement.Models;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalFiltrado, int TotalSinFiltros);

public sealed record AtraccionFiltroQuery(
    string? Ciudad = null,
    Guid? TipoCatGuid = null,
    Guid? SubtipoCatGuid = null,
    string? Etiqueta = null,
    string? Idioma = null,
    decimal? CalificacionMin = null,
    string? Horario = null,
    bool? Disponible = null,
    string OrdenarPor = "trending",
    int Page = 1,
    int Limit = 10);

public sealed record AtraccionAdminFiltroQuery(int Page = 1, int Limit = 10, string? Busqueda = null);

public sealed record AtraccionIndexRow(
    Guid AtGuid,
    string AtNombre,
    string? AtDescripcion,
    int AtTotalResenias,
    int? AtDuracionMinutos,
    decimal? AtPrecioReferencia,
    bool AtDisponible,
    string DesNombre,
    string DesPais,
    decimal PrecioMinimoTickets,
    double? CalificacionPromedio,
    bool TieneCuposProximos,
    IReadOnlyList<Guid> CatGuids,
    IReadOnlyList<string> IdiomaSnaps,
    IReadOnlyList<string> IncluyeSnaps,
    string? ImagenPrincipal,
    bool DispDisponibleHoy,
    DateOnly? DispProximaFecha,
    int? DispCupos);

public sealed record AtraccionDetalleRow(
    Guid AtGuid,
    string AtNombre,
    string? AtDescripcion,
    string DesNombre,
    string DesPais,
    int AtTotalResenias,
    double? CalificacionPromedio,
    string? AtDireccion,
    int? AtDuracionMinutos,
    string? AtPuntoEncuentro,
    decimal? AtPrecioReferencia,
    bool AtIncluyeAcompaniante,
    bool AtIncluyeTransporte,
    bool AtDisponible,
    List<AtraccionCategoriaRow> Categorias,
    List<AtraccionIdiomaRow> Idiomas,
    List<AtraccionImagenRow> Imagenes,
    List<AtraccionIncluyeRow> Incluyes,
    List<TicketRow> Tickets,
    List<HorarioRow> Horarios);

public sealed record AtraccionCategoriaRow(Guid CatGuid, Guid? ParentCatGuid);
public sealed record AtraccionIdiomaRow(Guid IdGuid, string IdDescripcion);
public sealed record AtraccionImagenRow(Guid ImgGuid, string ImgUrl, int Orden);
public sealed record AtraccionIncluyeRow(Guid IncGuid, string IncDescripcion);

public sealed record TicketRow(
    Guid TckGuid,
    string TckTitulo,
    decimal TckPrecio,
    string TckTipoParticipante,
    int TckCapacidadMaxima,
    int TckCuposDisponibles);

public sealed record HorarioRow(
    Guid HorGuid,
    Guid TckGuid,
    DateOnly HorFecha,
    DateOnly? HorFechaFin,
    TimeOnly HorHoraInicio,
    TimeOnly? HorHoraFin,
    int HorCuposDisponibles);

public sealed record HorarioProximoRow(
    Guid HorGuid,
    Guid TckGuid,
    DateOnly HorFecha,
    DateOnly? HorFechaFin,
    TimeOnly HorHoraInicio,
    TimeOnly? HorHoraFin,
    int HorCuposDisponibles,
    string TicketTitulo = "");

/// <summary>Fila mínima para calcular filtros (equivalente a cargar muchas atracciones en memoria).</summary>
public sealed record AtraccionFiltroSeedRow(
    Guid AtGuid,
    Guid DesGuid,
    string DesNombre,
    string DesPais,
    IReadOnlyList<Guid> CatGuids,
    IReadOnlyList<string> IdDescripciones,
    IReadOnlyList<string> IncDescripciones,
    double? CalificacionPromedio);

public sealed record AtraccionAdminRow(
    Guid AtGuid,
    Guid DesGuid,
    string DesNombreSnap,
    string DesPaisSnap,
    string? AtNumEstablecimiento,
    string AtNombre,
    string? AtDescripcion,
    int AtTotalResenias,
    string? AtDireccion,
    int? AtDuracionMinutos,
    string? AtPuntoEncuentro,
    decimal? AtPrecioReferencia,
    bool AtDisponible,
    char AtEstado,
    DateTime AtFechaIngreso);

/// <summary>Fila para calcular GET /atracciones/filtros (misma idea que el monolito: muestra de atracciones activas).</summary>
public sealed record AtraccionFiltroComputationRow(
    Guid AtGuid,
    Guid DesGuid,
    string DesNombreSnap,
    IReadOnlyList<Guid> CatGuids,
    IReadOnlyList<Guid> IdiomaGuids,
    IReadOnlyList<string> IdiomaSnaps,
    IReadOnlyList<string> IncluyeSnaps,
    double? CalificacionPromedio);

public sealed record AtraccionAdminCompletaRow(
    AtraccionAdminRow Base,
    IReadOnlyList<Guid> CategoriaGuids,
    IReadOnlyList<Guid> IdiomaGuids,
    IReadOnlyList<Guid> ImagenGuids,
    IReadOnlyList<Guid> IncluyeGuids,
    IReadOnlyList<string> IdiomaDescripciones,
    IReadOnlyList<string> ImagenUrls,
    IReadOnlyList<string> IncluyeDescripciones);

public sealed class AtraccionPersistModel
{
    public Guid? AtGuid { get; init; }
    public Guid DesGuid { get; init; }
    public string DesNombreSnap { get; init; } = string.Empty;
    public string DesPaisSnap { get; init; } = string.Empty;
    public string? NumEstablecimiento { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public string? Direccion { get; init; }
    public int? DuracionMinutos { get; init; }
    public string? PuntoEncuentro { get; init; }
    public decimal? PrecioReferencia { get; init; }
    public bool Disponible { get; init; } = true;
    public string Usuario { get; init; } = string.Empty;
    public string Ip { get; init; } = string.Empty;
    public IReadOnlyList<Guid> CategoriaGuids { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<Guid> IdiomaGuids { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<Guid> ImagenGuids { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<Guid> IncluyeGuids { get; init; } = Array.Empty<Guid>();
    public Dictionary<Guid, string> IdiomaDescripciones { get; init; } = new();
    public Dictionary<Guid, string> ImagenUrls { get; init; } = new();
    public Dictionary<Guid, string> IncluyeDescripciones { get; init; } = new();
}

public sealed record TicketAdminRow(
    Guid TckGuid,
    Guid AtGuid,
    string AtNombre,
    string TckTitulo,
    decimal TckPrecio,
    string TckTipoParticipante,
    int TckCapacidadMaxima,
    int TckCuposDisponibles,
    char TckEstado,
    DateTime TckFechaIngreso);

public sealed record TicketPersistModel(
    Guid? TckGuid,
    Guid AtGuid,
    string Titulo,
    decimal Precio,
    string TipoParticipante,
    int CapacidadMaxima,
    int CuposDisponibles,
    string Usuario,
    string Ip);

public sealed record HorarioAdminRow(
    Guid HorGuid,
    Guid TckGuid,
    Guid AtGuid,
    string AtNombre,
    string TckTitulo,
    int TckCapacidadMaxima,
    DateOnly HorFecha,
    DateOnly? HorFechaFin,
    TimeOnly HorHoraInicio,
    TimeOnly? HorHoraFin,
    int HorCuposDisponibles,
    char HorEstado,
    DateTime HorFechaIngreso);

public sealed record HorarioPersistModel(
    Guid? HorGuid,
    Guid TckGuid,
    DateOnly Fecha,
    DateOnly? FechaFin,
    TimeOnly HoraInicio,
    TimeOnly? HoraFin,
    int CuposDisponibles,
    string Usuario,
    string Ip);
