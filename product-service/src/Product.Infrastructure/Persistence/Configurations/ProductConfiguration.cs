using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Product.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Domain.Product>
{
    public void Configure(EntityTypeBuilder<Domain.Product> builder)
    {
        builder.Property(x => x.Name)
            .HasMaxLength(100);
        
        builder.Property(x => x.Description)
            .HasMaxLength(2_000);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.Amount)
            .IsConcurrencyToken();
        
        builder.HasIndex(x => x.CreatedAt);
    }
}