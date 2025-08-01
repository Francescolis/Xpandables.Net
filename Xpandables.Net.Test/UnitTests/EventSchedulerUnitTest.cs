﻿using System.Collections.Concurrent;
using System.Text.Json;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.Collections;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;
using Xpandables.Net.Repositories.Converters;
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

        await eventStore.AppendAsync(testEvent);

        // Act
        await eventScheduler.ScheduleAsync(CancellationToken.None);

        SchedulerOptions options = _serviceProvider.GetRequiredService<IOptions<SchedulerOptions>>().Value;
        options.IsEventSchedulerEnabled = false;

        Func<IQueryable<EntityIntegrationEvent>, IQueryable<EntityIntegrationEvent>> filterFunc = query =>
            query.Where(w => w.Status == EntityStatus.PUBLISHED)
                .OrderBy(o => o.EventVersion)
                .Take(10);

        // Assert
        List<IIntegrationEvent> events = await eventStore
            .FetchAsync(filterFunc, CancellationToken.None)
            .AsEventsAsync(CancellationToken.None)
            .OfType<IIntegrationEvent>()
            .ToListAsync(CancellationToken.None);

        events.Count.Should().Be(1);
        events.Should().ContainSingle(e => e.EventId == testEvent.EventId);
    }

    private record TestIntegrationEvent : IntegrationEvent
    {
    }
}

public class InMemoryEventStore : Repository, IEventStore
{
    private static readonly ConcurrentBag<IEntityEvent> _eventEntities = [];
    private readonly JsonSerializerOptions _options = DefaultSerializerOptions.Defaults;

    public Task AppendAsync(
        IEvent @event,
        CancellationToken cancellationToken = default)
    {
        IEventConverter eventConverter = EventConverter.GetConverterFor(@event);
        IEntityEvent entityEvent = eventConverter.ConvertTo(@event, _options);
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

    public override IAsyncEnumerable<TResult> FetchAsync<TEntity, TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>> filter,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AsyncEnumerable.Empty<TResult>();
        }

        IQueryable<TEntity> integrations = _eventEntities
            .OfType<TEntity>()
            .Select(CreateCopy)
            .AsQueryable();

        return filter(integrations).ToAsyncEnumerable();
    }

    public Task MarkAsProcessedAsync(
        EventProcessedInfo info,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<IEntityEventIntegration>? entities = _eventEntities
            .OfType<IEntityEventIntegration>()
            .Where(e => e.KeyId == info.EventId);

        entities.ForEach(e => e.SetStatus(EntityStatus.PUBLISHED));

        return Task.CompletedTask;
    }

    public Task MarkAsProcessedAsync(
        IEnumerable<EventProcessedInfo> infos,
        CancellationToken cancellationToken = default)
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

    private static TEntityEvent CreateCopy<TEntityEvent>(TEntityEvent entity)
        where TEntityEvent : class, IEntity
    {
        if (entity is not IEntityEvent entityEvent)
        {
            throw new NotSupportedException($"Unsupported entity event type: {typeof(TEntityEvent).FullName}");
        }

        JsonElement cloneElement = entityEvent.EventData.RootElement.Clone();
        JsonDocument cloneDocument = JsonDocument.Parse(cloneElement.GetRawText());
        IEntityEvent result = entityEvent switch
        {
            IEntityEventIntegration integration => new EntityIntegrationEvent
            {
                CreatedOn = integration.CreatedOn,
                DeletedOn = integration.DeletedOn,
                ErrorMessage = integration.ErrorMessage,
                EventData = cloneDocument,
                EventFullName = integration.EventFullName,
                EventName = integration.EventName,
                EventVersion = integration.EventVersion,
                KeyId = integration.KeyId,
                Status = integration.Status,
                UpdatedOn = integration.UpdatedOn
            },
            IEntityEventDomain domain => new EntityDomainEvent
            {
                AggregateId = domain.AggregateId,
                CreatedOn = domain.CreatedOn,
                DeletedOn = domain.DeletedOn,
                EventData = cloneDocument,
                EventFullName = domain.EventFullName,
                EventName = domain.EventName,
                EventVersion = domain.EventVersion,
                KeyId = domain.KeyId,
                Status = domain.Status,
                UpdatedOn = domain.UpdatedOn
            },
            _ => throw new NotSupportedException($"Unsupported entity event type: {entityEvent.GetType().FullName}")
        };

        return (TEntityEvent)result;
    }
}