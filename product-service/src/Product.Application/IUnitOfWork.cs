namespace Product.Application;

public interface IUnitOfWork
{
    Task Commit(CancellationToken cancellationToken = default);
}