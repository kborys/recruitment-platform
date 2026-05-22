using MediatR;

namespace Product.Application.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price
) : IRequest<Guid>;