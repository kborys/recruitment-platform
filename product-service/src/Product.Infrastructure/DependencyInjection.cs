using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application;
using Product.Application.Messaging;
using Product.Domain;
using Product.Infrastructure.Messaging;
using Product.Infrastructure.Persistence;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductDbContext>(dbContext =>
            {
                var connectionString = configuration.GetConnectionString("Default") ??
                                       throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
                dbContext.UseNpgsql(connectionString);
            })
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IProcessedEventStore, ProcessedEventStore>()
            .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProductDbContext>())
            .AddSingleton(TimeProvider.System);

        services.AddMessaging(configuration);
        
        return services;
    }
}