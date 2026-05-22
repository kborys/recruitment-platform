using Inventory.Domain;

namespace Inventory.UnitTest;

public class InventoryTests
{
    [Fact]
    public void Create_ShouldGenerateIdUsingVersion7()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantity = 10;
        var now = DateTime.UtcNow;
        var user = "test-user";

        // Act
        var inventory = Domain.Inventory.Create(productId, quantity, now, user);

        // Assert
        inventory.Id.Should().NotBeEmpty();
        inventory.ProductId.Should().Be(productId);
        inventory.Quantity.Should().Be(quantity);
        inventory.AddedAt.Should().Be(now);
        inventory.AddedBy.Should().Be(user);
    }

    [Fact]
    public void Create_WithEmptyProductId_ShouldThrowDomainException()
    {
        var act = () => Domain.Inventory.Create(Guid.Empty, 1, DateTime.UtcNow, "test-user");

        act.Should().Throw<DomainException>()
            .WithMessage("*ProductId*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Create_WithNonPositiveQuantity_ShouldThrowDomainException(int quantity)
    {
        var act = () => Domain.Inventory.Create(Guid.NewGuid(), quantity, DateTime.UtcNow, "test-user");

        act.Should().Throw<DomainException>()
            .WithMessage("*greater than zero*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Create_WithMissingAddedBy_ShouldThrowDomainException(string addedBy)
    {
        var act = () => Domain.Inventory.Create(Guid.NewGuid(), 1, DateTime.UtcNow, addedBy);

        act.Should().Throw<DomainException>()
            .WithMessage("*AddedBy*");
    }

    [Fact]
    public void Create_WithNullAddedBy_ShouldThrowDomainException()
    {
        var act = () => Domain.Inventory.Create(Guid.NewGuid(), 1, DateTime.UtcNow, null!);

        act.Should().Throw<DomainException>()
            .WithMessage("*AddedBy*");
    }
}
