using Inventory.Application.Abstractions;
using Inventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public class KnownProductRepository(InventoryDbContext context) : IKnownProductRepository
{
    public async Task<bool> Exists(Guid productId, CancellationToken cancellationToken = default)
    {
        return await context.KnownProducts.AnyAsync(x => x.Id == productId, cancellationToken);
    }

    public async Task<bool> InsertIfMissing(KnownProduct product, CancellationToken cancellationToken = default)
    {
        if (await Exists(product.Id, cancellationToken))
        {
            return false;
        }
        
        await context.KnownProducts.AddAsync(product, cancellationToken);
        return true;
    }
}