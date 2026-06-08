using System.Text.Json;
using System.Text.Json.Serialization;

namespace Atracciones.Contracts.Events;

public sealed class EventEnvelope<TPayload>
{
    [JsonPropertyName("event_id")]
    public Guid EventId { get; init; }

    [JsonPropertyName("event_type")]
    public string EventType { get; init; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    [JsonPropertyName("correlation_id")]
    public string CorrelationId { get; init; } = string.Empty;

    [JsonPropertyName("payload")]
    public TPayload Payload { get; init; } = default!;

    public static EventEnvelope<TPayload> Create(string eventType, TPayload payload, string correlationId, Guid? eventId = null) =>
        new()
        {
            EventId = eventId ?? Guid.NewGuid(),
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            CorrelationId = correlationId,
            Payload = payload,
        };

    public string ToJson() =>
        JsonSerializer.Serialize(this, EventJsonOptions.Default);

    public static EventEnvelope<TPayload>? FromJson(string json) =>
        JsonSerializer.Deserialize<EventEnvelope<TPayload>>(json, EventJsonOptions.Default);
}

public static class EventJsonOptions
{
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };
}
