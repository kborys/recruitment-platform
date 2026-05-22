using Inventory.Application.Abstractions;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(dbContext =>
            {
                var connectionString = configuration.GetConnectionString("Default") ??
                                       throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
                dbContext.UseNpgsql(connectionString);
            })
            .AddScoped<IKnownProductRepository, KnownProductRepository>()
            .AddScoped<IInventoryRepository, InventoryRepository>()
            .AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<InventoryDbContext>())
            .AddSingleton(TimeProvider.System);
            

        services.AddMessaging(configuration);
        
        return services;
    }
}