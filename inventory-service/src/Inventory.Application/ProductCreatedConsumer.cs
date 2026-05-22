using Contracts;
using Inventory.Application.Abstractions;
using Inventory.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Inventory.Application;

public class ProductCreatedConsumer(
    IKnownProductRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ProductCreatedConsumer> logger) : IConsumer<ProductCreatedEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var @event = context.Message;
        var knownProduct = KnownProduct.Create(@event.ProductId, timeProvider.GetUtcNow().UtcDateTime);
        if (await repository.InsertIfMissing(knownProduct))
        {
            await unitOfWork.Commit();
            logger.LogInformation(
                "KnownProduct {ProductId} registered from ProductCreatedEvent {EventId}",
                @event.ProductId, @event.EventId);
        }
        else
        {
            logger.LogDebug(
                "ProductCreatedEvent {EventId} for product {ProductId} ignored: already known",
                @event.EventId, @event.ProductId);
        }
    }
}
