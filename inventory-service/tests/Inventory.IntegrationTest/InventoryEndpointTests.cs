using System.Net;
using System.Net.Http.Json;
using Contracts;
using FluentAssertions;
using Inventory.Application.AddInventory;
using Inventory.Domain;
using Inventory.Infrastructure.Persistence;
using Inventory.IntegrationTest.Common;
using MassTransit.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.IntegrationTest;

public class InventoryEndpointTests(InventoryApiFactory factory) : IClassFixture<InventoryApiFactory>
{
    [Fact]
    public async Task PostInventory_ShouldReturn201AndPublishEvent()
    {
        var harness = factory.Services.GetRequiredService<ITestHarness>();
        // Arrange
        await using var seedScope = factory.Services.CreateAsyncScope();
        var dbContext = seedScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var product = KnownProduct.Create(Guid.NewGuid(), DateTime.UtcNow);
        dbContext.KnownProducts.Add(KnownProduct.Create(product.Id, DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        // Act
        using var client = factory.CreateAuthenticatedClient("test-user", "write");
        var response = await client.PostAsJsonAsync("/inventory", new
        {
            productId = product.Id,
            quantity = 5
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        await harness.ShouldHavePublished<ProductInventoryAddedEvent>(m => m.Context.Message.ProductId == product.Id
                                                                           && m.Context.Message.Quantity == 5, "because the stock level was modified");
    }

    [Fact]
    public async Task PostInventory_WithReadRoleOnly_ShouldReturn403()
    {
        // Arrange
        using var client = factory.CreateAuthenticatedClient("test-user", "read");

        // Act
        var response = await client.PostAsJsonAsync("/inventory", new
        {
            productId = Guid.NewGuid(),
            quantity = 5
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "POST /inventory requires the 'write' role");
    }

    [Fact]
    public async Task PostInventory_WithQuantityZero_ShouldReturn400WithValidationProblemDetails()
    {
        // Arrange
        using var client = factory.CreateAuthenticatedClient("test-user", "write");

        // Act
        var response = await client.PostAsJsonAsync("/inventory", new
        {
            productId = Guid.NewGuid(),
            quantity = 0
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Errors.Should().ContainKey(nameof(AddInventoryCommand.Quantity));
    }
}
