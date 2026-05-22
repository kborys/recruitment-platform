using MediatR;
using Product.Domain;

namespace Product.Application.GetProductById;

public class GetProductByIdQueryHandler(IProductRepository repository) : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.Get(request.Id, cancellationToken);
        return product is null 
            ? null
            : new ProductDto(product.Id, product.Name, product.Description, product.Price, product.Amount, product.CreatedAt, product.UpdatedAt);
    }
}