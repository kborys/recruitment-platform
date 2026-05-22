namespace Product.Domain;

public interface IProductRepository
{
    Task Insert(Product product, CancellationToken cancellationToken = default);
    Task<Product?> Get(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetForWrite(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> List(int skip, int take, CancellationToken cancellationToken = default);
}