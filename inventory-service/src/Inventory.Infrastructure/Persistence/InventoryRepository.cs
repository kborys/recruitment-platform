using Inventory.Application.Abstractions;

namespace Inventory.Infrastructure.Persistence;

public class InventoryRepository(InventoryDbContext context) : IInventoryRepository
{
    public async Task Insert(Domain.Inventory item, CancellationToken cancellationToken = default)
    {
        await context.AddAsync(item, cancellationToken);
    }
}