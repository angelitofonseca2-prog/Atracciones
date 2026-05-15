namespace Atracciones.MsCatalogos.DataManagement.Models;

public sealed record DestinoDto(
    Guid DesGuid,
    string Nombre,
    string Pais,
    string? ImagenUrl,
    char Estado);

public sealed record CategoriaDto(
    Guid CatGuid,
    string Nombre,
    Guid? ParentGuid,
    char Estado);

public sealed record IdiomaDto(
    Guid IdGuid,
    string Descripcion,
    char Estado);

public sealed record IncluyeDto(
    Guid IncGuid,
    string Descripcion,
    char Estado);

public sealed record ImagenDto(
    Guid ImgGuid,
    string Url,
    string? Descripcion,
    char Estado,
    DateTime FechaIngreso);
