using Microsoft.EntityFrameworkCore;
using Product.Application.Messaging;

namespace Product.Infrastructure.Persistence;

public class ProcessedEventStore(ProductDbContext context) : IProcessedEventStore
{
    public Task<bool> IsAlreadyProcessed(Guid eventId, CancellationToken ct = default)
    {
        return context.ProcessedEvents
            .AsNoTracking()
            .AnyAsync(p => p.EventId == eventId, ct);
    }

    public async Task MarkProcessed(Guid eventId, DateTime processedAt, CancellationToken ct = default)
    {
        await context.ProcessedEvents.AddAsync(
            new ProcessedEvent { EventId = eventId, ProcessedAt = processedAt },
            ct);
    }
}
