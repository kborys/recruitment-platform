namespace Product.UnitTest;

public class ProductTests
{
    [Fact]
    public void IncrementAmount_WithPositiveAmount_ShouldIncrement()
    {
        // Arrange
        var product = Domain.Product.Create("Test Product", null, 9.99m, DateTime.UtcNow);
        var initialAmount = product.Amount;
        var incrementAmount = 5;

        // Act
        product.IncrementAmount(incrementAmount, DateTime.UtcNow);

        // Assert
        product.Amount.Should().Be(initialAmount + incrementAmount);
    }

    [Fact]
    public void IncrementAmount_WithZeroAmount_ShouldThrowDomainException()
    {
        // Arrange
        var product = Domain.Product.Create("Test Product", null, 9.99m, DateTime.UtcNow);

        // Act
        var act = () => product.IncrementAmount(0, DateTime.UtcNow);

        // Assert
        act.Should().Throw<Domain.DomainException>()
            .WithMessage("*must be greater than zero*");
    }

    [Fact]
    public void IncrementAmount_WithNegativeAmount_ShouldThrowDomainException()
    {
        // Arrange
        var product = Domain.Product.Create("Test Product", null, 9.99m, DateTime.UtcNow);

        // Act
        var act = () => product.IncrementAmount(-5, DateTime.UtcNow);

        // Assert
        act.Should().Throw<Domain.DomainException>()
            .WithMessage("*must be greater than zero*");
    }
}
