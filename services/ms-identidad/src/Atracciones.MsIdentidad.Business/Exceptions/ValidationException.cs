namespace Atracciones.MsIdentidad.Business.Exceptions;

public sealed class ValidationException : Exception
{
    public IReadOnlyList<string> Errores { get; }

    public ValidationException(IReadOnlyList<string> errores)
        : base(string.Join(" ", errores))
        => Errores = errores;
}
