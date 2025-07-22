using System.Collections.Concurrent;
using System.Text.Json;

using Xpandables.Net.Collections;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Text;

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

    public IAsyncEnumerable<TEvent> FetchAsync<TEntityEvent, TEvent>(
        Func<IQueryable<TEntityEvent>, IAsyncQueryable<TEvent>> filter,
        CancellationToken cancellationToken = default)
        where TEntityEvent : class, IEntityEvent
        where TEvent : class, IEvent
    {
        IQueryable<TEntityEvent> integrations = _eventEntities
            .OfType<TEntityEvent>()
            .AsQueryable();

        return filter(integrations)
            .OfType<TEvent>();
    }

    public Task MarkAsProcessedAsync(
        EventProcessedInfo info, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        IEnumerable<IEntityEventIntegration> entities = _eventEntities
            .OfType<IEntityEventIntegration>()
            .Where(e => e.KeyId == info.EventId);

        entities.ForEach(e => e.SetStatus(EntityStatus.PUBLISHED));

        return Task.CompletedTask;
    }

    public Task MarkAsProcessedAsync(
        IEnumerable<EventProcessedInfo> infos, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }
        foreach (EventProcessedInfo info in infos)
        {
            MarkAsProcessedAsync(info, cancellationToken);
        }
        return Task.CompletedTask;
    }
}