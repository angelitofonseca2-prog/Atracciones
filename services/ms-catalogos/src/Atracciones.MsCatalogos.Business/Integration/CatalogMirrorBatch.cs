namespace Atracciones.MsCatalogos.Business.Integration;

public sealed class CatalogMirrorBatch
{
    public List<DestinoMirrorRow>? Destinos { get; set; }
    public List<CategoriaMirrorRow>? Categorias { get; set; }
    public List<IdiomaMirrorRow>? Idiomas { get; set; }
    public List<IncluyeMirrorRow>? Incluye { get; set; }
    public List<ImagenMirrorRow>? Imagenes { get; set; }
}

public sealed class DestinoMirrorRow
{
    public Guid DesGuid { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public char Estado { get; set; }
}

public sealed class CategoriaMirrorRow
{
    public Guid CatGuid { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid? ParentGuid { get; set; }
    public char Estado { get; set; }
}

public sealed class IdiomaMirrorRow
{
    public Guid IdGuid { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public char Estado { get; set; }
}

public sealed class IncluyeMirrorRow
{
    public Guid IncGuid { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public char Estado { get; set; }
}

public sealed class ImagenMirrorRow
{
    public Guid ImgGuid { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public char Estado { get; set; }
    public DateTime FechaIngreso { get; set; }
}
