using FluentValidation;
using Inventory.Application;
using Inventory.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Inventory.Infrastructure.Messaging;

public static class MessagingSetup
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemoryHarness = configuration.GetValue<bool>("Messaging:UseInMemoryHarness");

        if (useInMemoryHarness)
        {
            services.AddMassTransitTestHarness(bus =>
            {
                ConfigureConsumers(bus);
                bus.SetKebabCaseEndpointNameFormatter();
            });
            return services;
        }

        services.AddOptions<RabbitMqOptions>()
            .BindConfiguration(RabbitMqOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMassTransit(bus =>
        {
            bus.AddEntityFrameworkOutbox<InventoryDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
                outbox.QueryDelay = TimeSpan.FromSeconds(1);
                outbox.DuplicateDetectionWindow = TimeSpan.FromDays(1);
            });

            ConfigureConsumers(bus);
            bus.SetKebabCaseEndpointNameFormatter();

            bus.UsingRabbitMq((context, rmq) =>
            {
                var options = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                rmq.Host(options.Host, options.Port, options.VirtualHost, host =>
                {
                    host.Username(options.Username);
                    host.Password(options.Password);
                });

                rmq.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static void ConfigureConsumers(IBusRegistrationConfigurator bus)
    {
        bus.AddConsumer<ProductCreatedConsumer>(cfg =>
        {
            cfg.UseMessageRetry(retry =>
            {
                retry.Ignore<ValidationException>();
                retry.Exponential(5,
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(2));
            });
        });
    }
}
