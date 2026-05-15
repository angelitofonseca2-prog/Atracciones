namespace Atracciones.MsClientes.Business.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public sealed class UnauthorizedBusinessException : Exception
{
    public UnauthorizedBusinessException(string message) : base(message) { }
}

public sealed class ValidationException : Exception
{
    public IReadOnlyList<string> Errores { get; }
    public ValidationException(IReadOnlyList<string> errores) : base(string.Join(" ", errores)) => Errores = errores;
}
