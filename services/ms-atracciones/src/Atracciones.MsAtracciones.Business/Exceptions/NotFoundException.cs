namespace Atracciones.MsAtracciones.Business.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string recurso, Guid guid)
        : base($"{recurso} con GUID '{guid}' no existe o está inactivo.") { }
}
