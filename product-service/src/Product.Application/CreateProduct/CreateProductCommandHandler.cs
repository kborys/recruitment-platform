using Contracts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Product.Domain;

namespace Product.Application.CreateProduct;

public class CreateProductCommandHandler(
    IProductRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IPublishEndpoint eventPublisher,
    ILogger<CreateProductCommandHandler> logger) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Domain.Product.Create(request.Name, request.Description, request.Price, timeProvider.GetUtcNow().UtcDateTime);

        await repository.Insert(product, cancellationToken);
        await eventPublisher.Publish(new ProductCreatedEvent(Guid.NewGuid(),
                product.Id,
                product.Name,
                product.CreatedAt),
            cancellationToken);
        await unitOfWork.Commit(cancellationToken);

        logger.LogInformation(
            "Product {ProductId} created with name {Name}, price {Price}",
            product.Id, product.Name, product.Price);

        return product.Id;
    }
}
