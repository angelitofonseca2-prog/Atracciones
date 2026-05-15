using System.Text.Json;
using System.Text.Json.Serialization;
using Atracciones.Contracts.Auditoria.V1;
using Microsoft.Extensions.Logging;

namespace Atracciones.MsOrquestador.Business.Integration;

/// <summary>Invoca ms-auditoria sin bloquear la saga si el servicio falla.</summary>
public sealed class AuditoriaBestEffortPublisher
{
    private readonly AuditoriaService.AuditoriaServiceClient _client;
    private readonly ILogger<AuditoriaBestEffortPublisher> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public AuditoriaBestEffortPublisher(
        AuditoriaService.AuditoriaServiceClient client,
        ILogger<AuditoriaBestEffortPublisher> logger)
    {
        _client = client;
        _logger = logger;
    }

    public void Registrar(string tipo, string correlationId, object? payload)
    {
        _ = RegistrarInternoAsync(tipo, correlationId, payload);
    }

    private async Task RegistrarInternoAsync(string tipo, string correlationId, object? payload)
    {
        try
        {
            var json = payload is null ? "{}" : JsonSerializer.Serialize(payload, JsonOpts);
            await _client.RegistrarEventoAsync(new RegistrarEventoRequest
            {
                Tipo = tipo,
                CorrelationId = correlationId ?? string.Empty,
                PayloadJson = json,
            }, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Auditoría best-effort omitida ({Tipo})", tipo);
        }
    }
}
