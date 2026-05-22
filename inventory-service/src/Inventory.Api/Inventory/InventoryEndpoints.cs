namespace Inventory.Api.Inventory;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/inventory", AddInventoryEndpoint.Handle)
            .RequireAuthorization(p => p.RequireRole("write"));

        return app;
    }
}
