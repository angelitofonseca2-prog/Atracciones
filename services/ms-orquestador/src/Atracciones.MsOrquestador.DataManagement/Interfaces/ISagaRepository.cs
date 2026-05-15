namespace Atracciones.MsOrquestador.DataManagement.Interfaces;

public interface ISagaRepository
{
    Task<Guid> IniciarSagaAsync(string tipo, string correlationId, CancellationToken ct = default);

    Task RegistrarPasoAsync(
        Guid sagaId,
        string paso,
        string estado,
        string? requestPayload,
        string? responsePayload,
        string? error,
        CancellationToken ct = default);

    Task CompletarSagaAsync(Guid sagaId, string estadoFinal, CancellationToken ct = default);
}
