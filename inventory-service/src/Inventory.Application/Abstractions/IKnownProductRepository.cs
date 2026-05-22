using Inventory.Domain;

namespace Inventory.Application.Abstractions;

public interface IKnownProductRepository
{
    Task<bool> Exists(Guid productId, CancellationToken cancellationToken = default);
    Task<bool> InsertIfMissing(KnownProduct product, CancellationToken cancellationToken = default);
}