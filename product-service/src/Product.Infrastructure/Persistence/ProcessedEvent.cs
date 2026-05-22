namespace Product.Infrastructure.Persistence;

public class ProcessedEvent
{
    public Guid EventId { get; init; }
    public DateTime ProcessedAt { get; init; }
}
