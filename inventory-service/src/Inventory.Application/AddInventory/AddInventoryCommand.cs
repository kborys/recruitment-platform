using MediatR;

namespace Inventory.Application.AddInventory;

public record AddInventoryCommand(Guid ProductId, int Quantity) : IRequest<Guid>;