namespace Inventory.Api.Inventory;

public record AddInventoryRequest(Guid ProductId, int Quantity);