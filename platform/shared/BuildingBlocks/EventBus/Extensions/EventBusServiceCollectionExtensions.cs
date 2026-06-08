using Atracciones.Platform.BuildingBlocks.EventBus.Options;
using Atracciones.Platform.BuildingBlocks.EventBus.Outbox;
using Atracciones.Platform.BuildingBlocks.EventBus.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atracciones.Platform.BuildingBlocks.EventBus.Extensions;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddAtraccionesEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IServiceCollection>? configureConsumers = null)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<EvBusOptions>(configuration.GetSection(EvBusOptions.SectionName));

        services.AddSingleton<RabbitMqConnectionHolder>();
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddHostedService<RabbitMqTopologyInitializer>();
        services.AddHostedService<OutboxProcessorHostedService>();

        configureConsumers?.Invoke(services);

        return services;
    }
}
