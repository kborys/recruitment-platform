using Inventory.Application.Abstractions;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Domain.KnownProduct> KnownProducts { get; init; }
    public DbSet<Domain.Inventory> Inventories { get; init; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
        modelBuilder.AddTransactionalOutboxEntities();
        
        base.OnModelCreating(modelBuilder);
    }

    public async Task Commit(CancellationToken cancellationToken = default)
    {
        _ = await SaveChangesAsync(cancellationToken);
    }
}