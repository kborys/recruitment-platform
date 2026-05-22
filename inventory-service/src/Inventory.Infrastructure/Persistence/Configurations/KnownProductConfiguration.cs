using Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class KnownProductConfiguration : IEntityTypeConfiguration<KnownProduct>
{
    public void Configure(EntityTypeBuilder<KnownProduct> builder)
    {
    }
}