namespace Contracts;

public record ProductCreatedEvent(Guid EventId, Guid ProductId, string Name, DateTime OccurredAt);