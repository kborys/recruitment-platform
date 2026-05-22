using MediatR;

namespace Product.Application.ListProducts;

public record ListProductsQuery(int Page = 1, int PageSize = 50) : IRequest<IReadOnlyList<ProductDto>>;