namespace Inventory.Application.Abstractions;

public interface IUnitOfWork
{
    Task Commit(CancellationToken cancellationToken = default);
}