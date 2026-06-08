namespace Atracciones.Platform.BuildingBlocks.EventBus.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "atracciones";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}

public sealed class EvBusOptions
{
    public const string SectionName = "EvBus";

    public bool Enabled { get; set; }
}
