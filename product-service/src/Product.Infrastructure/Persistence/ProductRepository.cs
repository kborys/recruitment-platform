using Microsoft.EntityFrameworkCore;
using Product.Domain;

namespace Product.Infrastructure.Persistence;

public class ProductRepository(ProductDbContext context) : IProductRepository
{
    public async Task Insert(Domain.Product product, CancellationToken cancellationToken = default)
    {
        await context.AddAsync(product, cancellationToken);
    }

    public Task<Domain.Product?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<Domain.Product?> GetForWrite(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Products.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Product>> List(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }
}