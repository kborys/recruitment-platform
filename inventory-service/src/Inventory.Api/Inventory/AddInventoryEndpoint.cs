using Inventory.Application.AddInventory;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Inventory.Api.Inventory;

public static class AddInventoryEndpoint
{
    public static async Task<Created<AddInventoryResponse>> Handle(
        AddInventoryRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(new AddInventoryCommand(request.ProductId, request.Quantity), cancellationToken);
        return TypedResults.Created($"/inventory/{id}", new AddInventoryResponse(id));
    }
}
