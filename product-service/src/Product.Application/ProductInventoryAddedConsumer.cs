using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Product.Application.Messaging;
using Product.Domain;

namespace Product.Application;

public class ProductInventoryAddedConsumer(
    IProductRepository repository,
    IProcessedEventStore processedEventStore,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ProductInventoryAddedConsumer> logger
    ) : IConsumer<ProductInventoryAddedEvent>
{
    public async Task Consume(ConsumeContext<ProductInventoryAddedEvent> context)
    {
        var ct = context.CancellationToken;
        var eventId = context.Message.EventId;

        if (await processedEventStore.IsAlreadyProcessed(eventId, ct))
        {
            logger.LogInformation(
                "ProductInventoryAddedEvent {EventId} already processed, skipping",
                eventId);
            return;
        }

        var product = await repository.GetForWrite(context.Message.ProductId, ct);
        if (product is null)
        {
            throw new DomainException("Product not found");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        product.IncrementAmount(context.Message.Quantity, now);

        await processedEventStore.MarkProcessed(eventId, now, ct);
        await unitOfWork.Commit(ct);

        logger.LogInformation(
            "ProductInventoryAddedEvent {EventId} consumed: product {ProductId} amount incremented by {Quantity}",
            eventId, context.Message.ProductId, context.Message.Quantity);
    }
}
