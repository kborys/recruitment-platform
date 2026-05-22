using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Product.Application.ListProducts;

namespace Product.Api.Products;

public static class ListProductsEndpoint
{
    public static async Task<Ok<List<ProductResponse>>> Handle(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 50)
    {
        var products = await sender.Send(new ListProductsQuery(page, pageSize), cancellationToken);
        var response = products
            .Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.Amount))
            .ToList();
        return TypedResults.Ok(response);
    }
}
