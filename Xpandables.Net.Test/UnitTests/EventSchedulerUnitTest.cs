using System.Collections.Concurrent;
using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Collections;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Converters;
using Xpandables.Net.Repositories.Filters;
using Xpandables.Net.Text;

using AsyncEnumerable = System.Linq.AsyncEnumerable;

namespace Xpandables.Net.Test.UnitTests;

public sealed class EventSchedulerUnitTest
{
    private readonly IServiceProvider _serviceProvider;

    public EventSchedulerUnitTest()
    {
        ServiceCollection services = new();

        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddXPublisher();
        services.AddXSubscriber();
        services.AddXMessageQueue();
        services.AddXScheduler();
        services.AddOptions<EventOptions>();
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task EventScheduler_ShouldPublishEventsSuccessfully()
    {
        // Arrange
        IEventStore eventStore = _serviceProvider.GetRequiredService<IEventStore>();
        IScheduler eventScheduler = _serviceProvider.GetRequiredService<IScheduler>();

        TestIntegrationEvent testEvent = new() { EventId = Guid.CreateVersion7(), EventVersion = 1 };

        await eventStore.AppendAsync([testEvent]);

        // Act
        await eventScheduler.ScheduleAsync(CancellationToken.None);

        SchedulerOptions options = _serviceProvider.GetRequiredService<IOptions<SchedulerOptions>>().Value;
        options.IsEventSchedulerEnabled = false;

        EntityIntegrationEventFilter filter = new()
        {
            Predicate = x => x.Status == EntityStatus.PUBLISHED, PageIndex = 0, PageSize = 10
        };

        // Assert
        List<IEvent> events = await eventStore
            .FetchAsync(filter, CancellationToken.None)
            .ToListAsync(CancellationToken.None);

        filter.TotalCount.Should().Be(1);
        events.Should().ContainSingle(e => e.EventId == testEvent.EventId);
    }

    private record TestIntegrationEvent : IntegrationEvent
    {
    }
}

public class InMemoryEventStore : IEventStore
{
    private readonly EventConverterIntegration _eventConverter = new();
    private readonly ConcurrentBag<IEntityEvent> _eventEntities = [];
    private readonly JsonSerializerOptions _options = DefaultSerializerOptions.Defaults;

    public Task AppendAsync(
        IEvent @event,
        CancellationToken cancellationToken = default)
    {
        IEntityEvent entityEvent = _eventConverter.ConvertTo(@event, _options);
        _eventEntities.Add(entityEvent);

        return Task.CompletedTask;
    }

    public Task AppendAsync(
        IEnumerable<IEvent> events,
        CancellationToken cancellationToken = default)
    {
        events.ForEach(@event => AppendAsync(@event, cancellationToken));

        return Task.CompletedTask;
    }

    public IAsyncEnumerable<IEvent> FetchAsync(
        IEventFilter filter,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AsyncEnumerable.Empty<IEvent>();
        }

        IQueryable<IEntityEventIntegration> integrations = _eventEntities
            .OfType<IEntityEventIntegration>()
            .AsQueryable();

        IQueryable<IEntityEventIntegration>? filteredQuery = filter
            .Apply(integrations)
            .OfType<IEntityEventIntegration>();

        return AsyncEnumerable
            .ToAsyncEnumerable(filteredQuery
                .Select(entity =>
                    _eventConverter.ConvertFrom(entity, _options)));
    }

    public Task MarkAsProcessedAsync(
        EventProcessed eventPublished,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<IEntityEventIntegration>? entities = _eventEntities
            .OfType<IEntityEventIntegration>()
            .Where(e => e.KeyId == eventPublished.EventId);

        entities.ForEach(e => e.SetStatus(EntityStatus.PUBLISHED));

        return Task.CompletedTask;
    }
}