using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Product.Application.GetProductById;

namespace Product.Api.Products;

public static class GetProductByIdEndpoint
{
    public const string RouteName = "GetProductById";

    public static async Task<Results<Ok<ProductResponse>, NotFound>> Handle(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var product = await sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return product is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.Amount));
    }
}
