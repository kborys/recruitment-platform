using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.Behaviors;

namespace Product.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        
        services.AddValidatorsFromAssembly(assembly);
        
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
            
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        
        return services;
    }
}