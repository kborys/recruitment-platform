namespace Inventory.Application.Abstractions;

public interface IInventoryRepository
{
    Task Insert(Domain.Inventory item, CancellationToken cancellationToken = default);
}
