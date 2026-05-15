namespace Microservicio.Atracciones.Api.Models.Integration;

public sealed class CatalogMirrorIngressPayload
{
    public List<DestinoMirrorIngress>? Destinos { get; set; }
    public List<CategoriaMirrorIngress>? Categorias { get; set; }
    public List<IdiomaMirrorIngress>? Idiomas { get; set; }
    public List<IncluyeMirrorIngress>? Incluye { get; set; }
    public List<ImagenMirrorIngress>? Imagenes { get; set; }
}

public sealed class DestinoMirrorIngress
{
    public Guid DesGuid { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public char Estado { get; set; }
}

public sealed class CategoriaMirrorIngress
{
    public Guid CatGuid { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public Guid? ParentGuid { get; set; }
    public char Estado { get; set; }
}

public sealed class IdiomaMirrorIngress
{
    public Guid IdGuid { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public char Estado { get; set; }
}

public sealed class IncluyeMirrorIngress
{
    public Guid IncGuid { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public char Estado { get; set; }
}

public sealed class ImagenMirrorIngress
{
    public Guid ImgGuid { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public char Estado { get; set; }
    public DateTime FechaIngreso { get; set; }
}
