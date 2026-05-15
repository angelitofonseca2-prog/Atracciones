namespace Atracciones.MsAtracciones.Business.Dtos.Admin.Catalogos;

public sealed record DestinoResponseDto(string DesGuid, string Nombre, string Pais, string? ImagenUrl, char Estado);
public sealed record CategoriaResponseDto(string CatGuid, string Nombre, string? ParentGuid, string? ParentNombre);
public sealed record IdiomaResponseDto(string IdGuid, string Descripcion);
public sealed record IncluyeResponseDto(string IncluyeGuid, string Descripcion);
public sealed record ImagenResponseDto(string ImgGuid, string Url, string? Descripcion, char Estado, DateTime FechaIngreso);

public sealed record CrearDestinoRequestDto(string Nombre, string Pais, string? ImagenUrl);
public sealed record ActualizarDestinoRequestDto(string? Nombre, string? Pais, string? ImagenUrl);

public sealed record CrearCategoriaRequestDto(string Nombre, Guid? ParentGuid);
public sealed record ActualizarCategoriaRequestDto(string Nombre, Guid? ParentGuid);

public sealed record CrearIdiomaRequestDto(string Descripcion);
public sealed record ActualizarIdiomaRequestDto(string Descripcion);

public sealed record CrearIncluyeRequestDto(string Descripcion);
public sealed record ActualizarIncluyeRequestDto(string Descripcion);

public sealed record CrearImagenRequestDto(string Url, string? Descripcion);
public sealed record ActualizarImagenRequestDto(string? Url, string? Descripcion);
