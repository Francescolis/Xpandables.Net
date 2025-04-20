using System.Collections.Concurrent;
using System.Text.Json;

using Xpandables.Net.Collections;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Repositories.Filters;
using Xpandables.Net.Text;

namespace Xpandables.Net.Api;

public sealed class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentBag<IEventEntity> _eventEntities = [];
    private readonly EventConverterDomain _eventConverter = new();
    private readonly JsonSerializerOptions _options = DefaultSerializerOptions.Defaults;

    public Task AppendAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        IEventEntity eventEntity = _eventConverter.ConvertTo(@event, _options);
        _eventEntities.Add(eventEntity);

        return Task.CompletedTask;
    }

    public Task AppendAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default)
    {
        events.ForEach(@event => AppendAsync(@event, cancellationToken));

        return Task.CompletedTask;
    }

    public IAsyncEnumerable<IEvent> FetchAsync(IEventFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<IEventEntityDomain> integrations = _eventEntities
            .OfType<IEventEntityDomain>()
            .AsQueryable();

        IQueryable<IEventEntityDomain> filteredQuery = filter
            .Apply(integrations)
            .OfType<IEventEntityDomain>();

        return System.Linq.AsyncEnumerable
            .ToAsyncEnumerable(filteredQuery
                .Select(entity =>
                    _eventConverter.ConvertFrom(entity, _options)));
    }

    public Task MarkAsProcessedAsync(EventProcessed eventPublished, CancellationToken cancellationToken = default)
    {
        IEnumerable<IEventEntityIntegration> entities = _eventEntities
            .OfType<IEventEntityIntegration>()
            .Where(e => e.KeyId == eventPublished.EventId);

        entities.ForEach(e => e.SetStatus(EntityStatus.PUBLISHED));

        return Task.CompletedTask;
    }
}