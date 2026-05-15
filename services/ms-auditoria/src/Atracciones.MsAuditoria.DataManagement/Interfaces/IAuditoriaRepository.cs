namespace Atracciones.MsAuditoria.DataManagement.Interfaces;

public interface IAuditoriaRepository
{
    Task RegistrarEventoAsync(string tipo, string correlationId, string payloadJson, CancellationToken ct = default);
}
