using MassTransit;
using Microsoft.EntityFrameworkCore;
using Product.Application;

namespace Product.Infrastructure.Persistence;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Domain.Product> Products { get; init; }
    public DbSet<ProcessedEvent> ProcessedEvents { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);
        modelBuilder.AddTransactionalOutboxEntities();
        
        base.OnModelCreating(modelBuilder);
    }

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        _ = await SaveChangesAsync(cancellationToken);
    }
}