using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Api.Products;
using Product.Application.CreateProduct;
using Product.IntegrationTest.Common;

namespace Product.IntegrationTest;

public class ProductEndpointTests(ProductApiFactory factory) : IClassFixture<ProductApiFactory>
{
    [Fact]
    public async Task PostProductWithWriteRole_ShouldReturn201WithLocation()
    {
        // arrange
        using var client = factory.CreateAuthenticatedClient("test-user", "write");

        // act
        var createProductRequest = new
        {
            name = "Test Product",
            price = 7.99m,
        };
        var response = await client.PostAsJsonAsync("/products", createProductRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<CreateProductResponse>();
        created!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostProduct_WithoutAuth_ShouldReturn401()
    {
        // arrange
        using var client = factory.CreateClient();
        var createProductRequest = new
        {
            name = "Test Product",
            price = 7.99m,
        };

        // act
        var response = await client.PostAsJsonAsync("/products", createProductRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostProduct_WithReadRoleOnly_ShouldReturn403()
    {
        // arrange
        using var client = factory.CreateAuthenticatedClient("test-user", "read");
        var createProductRequest = new
        {
            name = "Test Product",
            price = 7.99m,
        };

        // act
        var response = await client.PostAsJsonAsync("/products", createProductRequest);

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden, "POST /products requires the 'write' role");
    }

    [Fact]
    public async Task PostProduct_WithPriceZero_ShouldReturn400WithValidationProblemDetails()
    {
        // arrange
        using var client = factory.CreateAuthenticatedClient("test-user", "write");

        // act
        var response = await client.PostAsJsonAsync("/products", new
        {
            name = "Test Product",
            price = 0m,
        });

        // assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Errors.Should().ContainKey(nameof(CreateProductCommand.Price));
    }
}