using System.Collections.Concurrent;
using System.Text.Json;

using Xpandables.Net.Collections;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Repositories.Filters;
using Xpandables.Net.Text;

using AsyncEnumerable = System.Linq.AsyncEnumerable;

namespace Xpandables.Net.Api;

public sealed class InMemoryEventStore : IEventStore
{
    private readonly EventConverterDomain _eventConverter = new();
    private readonly ConcurrentBag<IEntityEvent> _eventEntities = [];
    private readonly JsonSerializerOptions _options = DefaultSerializerOptions.Defaults;

    public Task AppendAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        IEntityEvent entityEvent = _eventConverter.ConvertTo(@event, _options);
        _eventEntities.Add(entityEvent);

        return Task.CompletedTask;
    }

    public Task AppendAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        events.ForEach(@event => AppendAsync(@event, cancellationToken));

        return Task.CompletedTask;
    }

    public IAsyncEnumerable<IEvent> FetchAsync(IEventFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<IEntityEventDomain> integrations = _eventEntities
            .OfType<IEntityEventDomain>()
            .AsQueryable();

        IQueryable<IEntityEventDomain> filteredQuery = filter
            .Apply(integrations)
            .OfType<IEntityEventDomain>();

        return AsyncEnumerable
            .ToAsyncEnumerable(filteredQuery
                .Select(entity =>
                    _eventConverter.ConvertFrom(entity, _options)));
    }

    public Task MarkAsProcessedAsync(EventProcessed eventPublished, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        IEnumerable<IEntityEventIntegration> entities = _eventEntities
            .OfType<IEntityEventIntegration>()
            .Where(e => e.KeyId == eventPublished.EventId);

        entities.ForEach(e => e.SetStatus(EntityStatus.PUBLISHED));

        return Task.CompletedTask;
    }
}