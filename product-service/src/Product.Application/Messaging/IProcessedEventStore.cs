namespace Product.Application.Messaging;

public interface IProcessedEventStore
{
    Task<bool> IsAlreadyProcessed(Guid eventId, CancellationToken ct = default);
    Task MarkProcessed(Guid eventId, DateTime processedAt, CancellationToken ct = default);
}
