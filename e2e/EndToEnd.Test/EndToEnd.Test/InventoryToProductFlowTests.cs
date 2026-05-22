using System.Net.Http.Json;
using EndToEnd.Test.Common;
using FluentAssertions;
using Inventory.Api.Inventory;
using Product.Api.Products;

namespace EndToEnd.Test;

public class InventoryToProductFlowTests(E2EEnvironment environment) : IClassFixture<E2EEnvironment>
{
    [Fact]
    public async Task PostInventory_ProductServiceShouldIncreaseProductAmount()
    {
        // Arrange
        using var productClient = environment.ProductApiFactory.CreateAuthenticatedClient("e2e-user", "write", "read");
        using var inventoryClient = environment.InventoryApiFactory.CreateAuthenticatedClient("e2e-user", "write");
        
        // Act
        // 1: Create a product within product-service
        var createdProductResponse = await productClient.PostAsJsonAsync("/products", new { name = "E2E Product", price = 9.99m });
        createdProductResponse.EnsureSuccessStatusCode();
        var created = await createdProductResponse.Content.ReadFromJsonAsync<CreateProductResponse>();
        var productId = created!.Id;
        productId.Should().NotBeEmpty();

        // 2.1: wait until product propagated to inventory-service
        // 2.2: add inventory entry
        var inventoryResponse = await Eventually.WaitFor(async () => await inventoryClient.PostAsJsonAsync("/inventory",
                new
                {
                    productId,
                    quantity = 5
                }),
            r => r.IsSuccessStatusCode);
        var inventoryEntry = await inventoryResponse.Content.ReadFromJsonAsync<AddInventoryResponse>();
        inventoryEntry!.Id.Should().NotBeEmpty();

        // Assert: Wait until inventory propagated to product-service
        await Eventually.WaitFor(async () =>
            {
                var response = await productClient.GetAsync($"/products/{productId}");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<ProductResponse>();
            },
            r => r?.Amount == 5);
    }
}