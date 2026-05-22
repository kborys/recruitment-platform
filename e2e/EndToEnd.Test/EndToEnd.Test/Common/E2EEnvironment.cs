using Inventory.IntegrationTest;
using Product.IntegrationTest;
using Testcontainers.RabbitMq;

namespace EndToEnd.Test.Common;

public class E2EEnvironment : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder("rabbitmq:4.3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public ProductApiFactory ProductApiFactory { get; init; }
    public InventoryApiFactory InventoryApiFactory { get; init; }

    public E2EEnvironment()
    {
        ProductApiFactory = new ProductApiFactory().WithSharedRabbitMq(_rabbit);
        InventoryApiFactory = new InventoryApiFactory().WithSharedRabbitMq(_rabbit);
    }

    public async Task InitializeAsync()
    {
        await _rabbit.StartAsync();
        await Task.WhenAll(ProductApiFactory.InitializeAsync(), InventoryApiFactory.InitializeAsync());
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(ProductApiFactory.DisposeAsync(), InventoryApiFactory.DisposeAsync());
        await _rabbit.DisposeAsync();
    }
}
