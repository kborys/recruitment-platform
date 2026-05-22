using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Domain.Inventory>
{
    public void Configure(EntityTypeBuilder<Domain.Inventory> builder)
    {
        builder.Property(x => x.AddedBy)
            .HasMaxLength(255);
    }
}