namespace Inventory.Domain;

public class Inventory
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public DateTime AddedAt { get; init; }
    public string AddedBy { get; init; } = null!;

    private Inventory() { }

    private Inventory(Guid id, Guid productId, int quantity, DateTime addedAt, string addedBy)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        AddedAt = addedAt;
        AddedBy = addedBy;
    }

    public static Inventory Create(Guid productId, int quantity, DateTime addedAt, string addedBy)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("ProductId is required");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero");
        }

        if (string.IsNullOrWhiteSpace(addedBy))
        {
            throw new DomainException("AddedBy is required");
        }

        return new Inventory(Guid.CreateVersion7(new DateTimeOffset(addedAt, TimeSpan.Zero)), productId, quantity, addedAt, addedBy);
    }
}
