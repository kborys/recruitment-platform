namespace Product.Api.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/products");

        group.MapPost("/", CreateProductEndpoint.Handle)
            .RequireAuthorization(p => p.RequireRole("write"));

        group.MapGet("/", ListProductsEndpoint.Handle)
            .RequireAuthorization(p => p.RequireRole("read"));

        group.MapGet("/{id:guid}", GetProductByIdEndpoint.Handle)
            .RequireAuthorization(p => p.RequireRole("read"))
            .WithName(GetProductByIdEndpoint.RouteName);

        return app;
    }
}
