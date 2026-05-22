using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Product.Application.CreateProduct;

namespace Product.Api.Products;

public static class CreateProductEndpoint
{
    public static async Task<CreatedAtRoute<CreateProductResponse>> Handle(
        CreateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(new CreateProductCommand(request.Name, request.Description, request.Price), cancellationToken);
        return TypedResults.CreatedAtRoute(new CreateProductResponse(id), GetProductByIdEndpoint.RouteName, new { id });
    }
}
