namespace Microservicio.Atracciones.Business.Interfaces.Integration;

/// <summary>
/// Replica datos de cliente del monolito hacia ms-clientes (esquema crm) por HTTP interno.
/// </summary>
public interface IClienteCrmSyncPublisher
{
    /// <summary>
    /// Si el servicio está deshabilitado o falla la llamada, solo se registra en log (no lanza).
    /// </summary>
    Task EspejarAsync(ClienteCrmEspejo espejo, CancellationToken cancellationToken = default);
}

public sealed record ClienteCrmEspejo(
    Guid UsuGuid,
    string TipoIdentificacion,
    string NumeroIdentificacion,
    string? Nombres,
    string? Apellidos,
    string? RazonSocial,
    string Correo,
    string? Telefono,
    string? Direccion,
    string CreadoPor,
    string IpCreador);
