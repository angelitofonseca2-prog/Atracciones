namespace Atracciones.MsIdentidad.Business.Exceptions;

public sealed class UnauthorizedBusinessException : Exception
{
    public UnauthorizedBusinessException(string message) : base(message) { }
}
