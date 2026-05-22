using MediatR;

namespace Product.Application.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;