using System.Net.Http.Json;
using Contracts;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Product.Api.Products;
using Product.Application;
using Product.Infrastructure.Persistence;
using Product.IntegrationTest.Common;

namespace Product.IntegrationTest;

public class ProductInventoryAddedConsumerTests(ProductApiFactory factory) : IClassFixture<ProductApiFactory>
{
    [Fact]
    public async Task WhenSameEventDeliveredTwice_ProductAmountShouldUpdateOnlyOnce()
    {
        // Arrange
        var harness = factory.Services.GetRequiredService<ITestHarness>();

        using var client = factory.CreateAuthenticatedClient("test-user", "write", "read");

        var createProductResponse = await client.PostAsJsonAsync("/products", new
        {
            name = "Test Product",
            price = 7.99m
        });
        createProductResponse.EnsureSuccessStatusCode();
        var created = await createProductResponse.Content.ReadFromJsonAsync<CreateProductResponse>();
        var productId = created!.Id;

        var eventId = Guid.NewGuid();
        var @event = new ProductInventoryAddedEvent(eventId, productId, 1, DateTime.UtcNow);

        // Act
        await harness.Bus.Publish(@event);
        await harness.Bus.Publish(@event);

        // Wait for both deliveries to be consumed before asserting idempotency.
        var consumerHarness = harness.GetConsumerHarness<ProductInventoryAddedConsumer>();
        var consumedTwo = AsyncElementListExtensions.Take(consumerHarness.Consumed.SelectAsync<ProductInventoryAddedEvent>(), 2);
        (await consumedTwo.Count()).Should().Be(2);

        // Assert
        var product = await ReadProduct(client, productId);
        product.Amount.Should().Be(1, "duplicate delivery of the same event should be de-duplicated");

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var processedCount = await dbContext.ProcessedEvents.CountAsync(p => p.EventId == eventId);
        processedCount.Should().Be(1, "dedup table should record the event exactly once");
    }

    private static async Task<ProductResponse> ReadProduct(HttpClient client, Guid productId)
    {
        var response = await client.GetAsync($"/products/{productId}");
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        return product!;
    }
}
