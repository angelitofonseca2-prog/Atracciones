namespace Atracciones.MsOrquestador.DataManagement.Interfaces;

public interface IIdempotencyRepository
{
    Task<string?> ObtenerRespuestaSiExisteAsync(string idempotencyKey, string route, string bodyHash, CancellationToken ct = default);

    Task GuardarRespuestaAsync(string idempotencyKey, string route, string bodyHash, string responseJson, CancellationToken ct = default);
}
