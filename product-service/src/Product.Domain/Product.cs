namespace Product.Domain;

public class Product
{
    public Guid Id { get; init; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Amount { get; private set; } = 0;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; private set; }

    private Product() { }

    private Product(Guid id, string name, string? description, decimal price, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static Product Create(string name, string? description, decimal price, DateTime createdAt)
    {
        return new Product(Guid.CreateVersion7(new DateTimeOffset(createdAt, TimeSpan.Zero)), name.Trim(), description?.Trim(), price, createdAt);
    }

    public void IncrementAmount(int amount, DateTime updatedAt)
    {
        if (amount <= 0)
        {
            throw new DomainException("Amount  must be greater than zero");
        }

        Amount += amount;
        UpdatedAt = updatedAt;
    }
}
