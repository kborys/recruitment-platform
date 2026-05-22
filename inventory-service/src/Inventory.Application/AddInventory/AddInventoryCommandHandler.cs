using Contracts;
using Inventory.Application.Abstractions;
using Inventory.Domain;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.AddInventory;

public class AddInventoryCommandHandler(
    TimeProvider timeProvider,
    ICurrentUser currentUser,
    IKnownProductRepository knownProducts,
    IInventoryRepository repository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint eventPublisher,
    ILogger<AddInventoryCommandHandler> logger
    ) : IRequestHandler<AddInventoryCommand, Guid>
{
    public async Task<Guid> Handle(AddInventoryCommand request, CancellationToken cancellationToken)
    {
        if (!await knownProducts.Exists(request.ProductId, cancellationToken))
        {
            throw new DomainException($"Unknown product {request.ProductId}. Create the product first.");
        }

        var inventory = Domain.Inventory.Create(request.ProductId, request.Quantity, timeProvider.GetUtcNow().UtcDateTime, currentUser.UserName);

        await repository.Insert(inventory, cancellationToken);
        await eventPublisher.Publish(new ProductInventoryAddedEvent(Guid.NewGuid(),
                inventory.ProductId,
                inventory.Quantity,
                inventory.AddedAt),
            cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        logger.LogInformation(
            "Inventory entry {InventoryId} created for product {ProductId}, quantity {Quantity}, by {AddedBy}",
            inventory.Id, inventory.ProductId, inventory.Quantity, inventory.AddedBy);

        return inventory.Id;
    }
}
