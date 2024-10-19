﻿using System.Collections.Concurrent;
using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Collections;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events;
using Xpandables.Net.Events.Converters;
using Xpandables.Net.Events.Filters;
using Xpandables.Net.Repositories;
using Xpandables.Net.Text;

namespace Xpandables.Net.Test.UnitTests;
public sealed class EventSchedulerUnitTest
{
    private readonly IServiceProvider _serviceProvider;

    public EventSchedulerUnitTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IEventStore, InMemoryEventStore>();
        services.AddXEventPublisher();
        services.AddXEventSubscriber();
        services.AddXEventScheduler();
        services.Configure<EventOptions>(EventOptions.Default);
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task EventScheduler_ShouldPublishEventsSuccessfully()
    {
        // Arrange
        var eventStore = _serviceProvider.GetRequiredService<IEventStore>();
        var eventPublisher = _serviceProvider.GetRequiredService<IEventPublisher>();
        var eventScheduler = _serviceProvider.GetRequiredService<IEventScheduler>();

        var testEvent = new TestEventIntegration
        {
            EventId = Guid.NewGuid(),
            EventVersion = 1
        };

        await eventStore.AppendAsync([testEvent]);

        // Act
        await eventScheduler.ScheduleAsync();

        // Assert
        var events = await eventStore.FetchAsync(new EventEntityFilterIntegration
        {
            Predicate = x => x.Status == EntityStatus.PUBLISHED,
            PageIndex = 0,
            PageSize = 10
        })
        .ToListAsync();

        events.Should().ContainSingle(e => e.EventId == testEvent.EventId);
    }

    private record TestEventIntegration : EventIntegration
    { }

}


public class InMemoryEventStore : IEventStore
{
    private readonly ConcurrentBag<IEventEntity> _eventEntities = [];
    private readonly EventConverterIntegration _eventConverter = new();
    private readonly JsonSerializerOptions _options = DefaultSerializerOptions.Defaults;
    public Task AppendAsync(
        IEnumerable<IEvent> events,
        CancellationToken cancellationToken = default)
    {
        events.ForEach(@event =>
        {
            IEventEntity eventEntity = _eventConverter.ConvertTo(@event, _options);
            _eventEntities.Add(eventEntity);
        });

        return Task.CompletedTask;
    }

    public IAsyncEnumerable<IEvent> FetchAsync(
        IEventFilter filter,
        CancellationToken cancellationToken = default)
    {
        IQueryable<IEventEntityIntegration> integrations = _eventEntities
            .OfType<IEventEntityIntegration>()
            .AsQueryable();

        var filteredQuery = filter
            .Apply(integrations)
            .OfType<IEventEntityIntegration>();

        return System.Linq.AsyncEnumerable
            .ToAsyncEnumerable(filteredQuery
                .Select(entity =>
                    _eventConverter.ConvertFrom(entity, _options)));
    }

    public Task MarkAsPublishedAsync(
        IEnumerable<EventPublished> events,
        CancellationToken cancellationToken = default)
    {
        var entities = _eventEntities
            .OfType<IEventEntityIntegration>()
            .Where(e => events.Any(ep => ep.EventId == e.Id));

        entities.ForEach(e => e.SetStatus(EntityStatus.PUBLISHED));

        return Task.CompletedTask;
    }
}