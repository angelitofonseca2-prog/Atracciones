namespace Atracciones.MsCatalogos.Business.Exceptions;

public sealed class ValidationException : Exception
{
    public IReadOnlyList<string> Errores { get; }
    public ValidationException(IEnumerable<string> errores) : base(string.Join("; ", errores))
        => Errores = errores.ToList();
}

public sealed class NotFoundException : Exception
{
    public NotFoundException(string recurso, object id) : base($"{recurso} no encontrado: {id}") { }
}

public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
