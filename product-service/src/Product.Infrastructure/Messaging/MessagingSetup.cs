using Contracts;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Product.Application;
using Product.Infrastructure.Persistence;

namespace Product.Infrastructure.Messaging;

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
            bus.AddEntityFrameworkOutbox<ProductDbContext>(outbox =>
            {
                outbox.UsePostgres();
                outbox.UseBusOutbox();
                outbox.QueryDelay = TimeSpan.FromSeconds(1);
                outbox.DuplicateDetectionWindow = TimeSpan.FromDays(1);
            });

            ConfigureConsumers(bus);
            bus.SetKebabCaseEndpointNameFormatter();

            bus.UsingRabbitMq((busContext, rmq) =>
            {
                var options = busContext.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                rmq.Host(options.Host, options.Port, options.VirtualHost, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });

                rmq.ConfigureEndpoints(busContext);
            });
        });

        return services;
    }

    private static void ConfigureConsumers(IBusRegistrationConfigurator bus)
    {
        bus.AddConsumer<ProductInventoryAddedConsumer>(cfg =>
        {
            cfg.UseMessageRetry(retry =>
            {
                retry.Ignore<ValidationException>();
                retry.Exponential(5,
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(2));
            });

            cfg.Message<ProductInventoryAddedEvent>(m =>
            {
                m.UsePartitioner(16, context => context.Message.ProductId);
            });
        });
    }
}
