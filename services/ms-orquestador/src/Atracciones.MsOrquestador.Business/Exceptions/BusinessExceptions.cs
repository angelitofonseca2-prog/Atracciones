namespace Atracciones.MsOrquestador.Business.Exceptions;

public sealed class ValidationOrchestadorException : Exception
{
    public IReadOnlyList<string> Errores { get; }

    public ValidationOrchestadorException(IEnumerable<string> errores)
        : base(string.Join("; ", errores))
    {
        Errores = errores.ToList();
    }
}

public sealed class NotFoundOrchestadorException : Exception
{
    public NotFoundOrchestadorException(string message) : base(message) { }
}

public sealed class ConflictOrchestadorException : Exception
{
    public ConflictOrchestadorException(string message) : base(message) { }
}

public sealed class ForbiddenOrchestadorException : Exception
{
    public ForbiddenOrchestadorException(string message) : base(message) { }
}

public sealed class ServiceUnavailableOrchestadorException : Exception
{
    public ServiceUnavailableOrchestadorException(string message) : base(message) { }
}
