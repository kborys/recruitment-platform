namespace Inventory.Domain;

public class KnownProduct
{
    public Guid Id { get; init; }
    public DateTime AddedAt { get; init; }

    private KnownProduct() { }

    private KnownProduct(Guid id, DateTime addedAt)
    {
        Id = id;
        AddedAt = addedAt;
    }

    public static KnownProduct Create(Guid id, DateTime addedAt)
    {
        return new KnownProduct(id, addedAt);
    }
}
