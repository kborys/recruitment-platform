using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Product.IntegrationTest.Common;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace Product.IntegrationTest;

public class ProductApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18.4-alpine")
        .WithDatabase("product")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private RabbitMqContainer? _rabbit;

    public ProductApiFactory WithSharedRabbitMq(RabbitMqContainer rabbitMqContainer)
    {
        _rabbit = rabbitMqContainer;
        return this;
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = _postgres.GetConnectionString();
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("Database:AutoMigrate", "true");

        // jwt setup - match JwtTestTokenFactory so issued tokens validate
        builder.UseSetting("Jwt:SigningKey", JwtTestTokenFactory.SigningKey);
        builder.UseSetting("Jwt:Issuer", JwtTestTokenFactory.Issuer);
        builder.UseSetting("Jwt:Audience", JwtTestTokenFactory.Audience);

        if (_rabbit is null)
        {
            // MassTransit in-memory test harness instead of real RabbitMQ
            builder.UseSetting("Messaging:UseInMemoryHarness", "true");
        }
        else
        {
            // shared RabbitMQ container
            var rabbitPort = _rabbit.GetMappedPublicPort(5672).ToString(CultureInfo.InvariantCulture);
            builder.UseSetting("RabbitMq:Host", _rabbit.Hostname);
            builder.UseSetting("RabbitMq:Port", rabbitPort);
            builder.UseSetting("RabbitMq:Username", "guest");
            builder.UseSetting("RabbitMq:Password", "guest");
            builder.UseSetting("RabbitMq:VirtualHost", "/");
        }

        base.ConfigureWebHost(builder);
    }
}
