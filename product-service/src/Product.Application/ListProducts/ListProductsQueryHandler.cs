using MediatR;
using Product.Domain;

namespace Product.Application.ListProducts;

public class ListProductsQueryHandler(IProductRepository repository) : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;
        var result = await repository.List(skip, request.PageSize, cancellationToken);
        return result
            .Select(x => new ProductDto(x.Id, x.Name, x.Description, x.Price, x.Amount, x.CreatedAt, x.UpdatedAt))
            .ToArray();
    }
}