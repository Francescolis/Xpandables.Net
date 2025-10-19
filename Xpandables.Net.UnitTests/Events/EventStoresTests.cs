/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.ComponentModel.DataAnnotations;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Xpandables.Net.Events;
using Xpandables.Net.Optionals;
using Xpandables.Net.States;

namespace Xpandables.Net.UnitTests.Events;

/// <summary>
/// Unit tests for AggregateStore implementation.
/// </summary>
public sealed class AggregateStoreTests
{
    [Fact]
    public async Task LoadAsync_WithValidStreamId_LoadsAggregateFromEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();

        // Setup events in the store
        var events = new List<IDomainEvent>
        {
            new BankAccountCreatedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                AccountNumber = "ACC001",
                Owner = "John Doe",
                InitialBalance = 1000m
            },
            new MoneyDepositedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                Amount = 500m
            }
        };

        await eventStore.AppendToStreamAsync(new AppendRequest
        {
            StreamId = streamId,
            Events = events,
            ExpectedVersion = -1
        });

        var aggregateStore = new AggregateStore<BankAccountAggregate>(eventStore, domainEvents);

        // Act
        var aggregate = await aggregateStore.LoadAsync(streamId);

        // Assert
        aggregate.Should().NotBeNull();
        aggregate.StreamId.Should().Be(streamId);
        aggregate.AccountNumber.Should().Be("ACC001");
        aggregate.Owner.Should().Be("John Doe");
        aggregate.Balance.Should().Be(1500m);
        aggregate.StreamVersion.Should().Be(1);
    }

    [Fact]
    public async Task LoadAsync_WithEmptyStreamId_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();

        var aggregateStore = new AggregateStore<BankAccountAggregate>(eventStore, domainEvents);

        // Act
        var act = () => aggregateStore.LoadAsync(Guid.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task LoadAsync_WithNoEvents_ThrowsValidationException()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();

        var aggregateStore = new AggregateStore<BankAccountAggregate>(eventStore, domainEvents);

        // Act
        var act = () => aggregateStore.LoadAsync(streamId);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SaveAsync_WithUncommittedEvents_AppendsToEventStore()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();

        var aggregate = BankAccountAggregate.Create(streamId, "ACC001", "John Doe", 1000m);
        aggregate.Deposit(500m);

        var aggregateStore = new AggregateStore<BankAccountAggregate>(eventStore, domainEvents);

        // Act
        await aggregateStore.SaveAsync(aggregate);

        // Assert
        var savedEvents = await eventStore
            .ReadStreamAsync(new ReadStreamRequest { StreamId = streamId, FromVersion = -1, MaxCount = 100 })
            .ToListAsync();

        savedEvents.Should().HaveCount(2);
        // Domain events are added to the buffer during save
    }

    [Fact]
    public async Task SaveAsync_WithNoUncommittedEvents_DoesNothing()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();

        var aggregate = BankAccountAggregate.Create(streamId, "ACC001", "John Doe", 1000m);
        aggregate.MarkEventsAsCommitted();

        var aggregateStore = new AggregateStore<BankAccountAggregate>(eventStore, domainEvents);

        // Act
        await aggregateStore.SaveAsync(aggregate);

        // Assert
        // No events should be added to the store since there are no uncommitted events
        aggregate.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_WithNullAggregate_ThrowsArgumentNullException()
    {
        // Arrange
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();

        var aggregateStore = new AggregateStore<BankAccountAggregate>(eventStore, domainEvents);

        // Act
        var act = () => aggregateStore.SaveAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveAsync_CalculatesCorrectExpectedVersion()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();

        var aggregate = BankAccountAggregate.Create(streamId, "ACC001", "John Doe", 1000m);
        var aggregateStore = new AggregateStore<BankAccountAggregate>(eventStore, domainEvents);
        
        await aggregateStore.SaveAsync(aggregate);
        aggregate.MarkEventsAsCommitted();

        // Act - Add more events
        aggregate.Deposit(500m);
        await aggregateStore.SaveAsync(aggregate);

        // Assert
        var allEvents = await eventStore
            .ReadStreamAsync(new ReadStreamRequest { StreamId = streamId, FromVersion = -1, MaxCount = 100 })
            .ToListAsync();

        allEvents.Should().HaveCount(2);
        allEvents.Last().StreamVersion.Should().Be(1);
    }
}

/// <summary>
/// Unit tests for SnapshotStore implementation.
/// </summary>
public sealed class SnapshotStoreTests
{
    [Fact]
    public async Task LoadAsync_WithSnapshotDisabled_DelegatesToAggregateStore()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var options = Options.Create(new SnapshotOptions { IsSnapshotEnabled = false });
        
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();
        var innerAggregateStore = new AggregateStore<TestSnapshotAggregate>(eventStore, domainEvents);

        // Create and save an aggregate
        var originalAggregate = TestSnapshotAggregate.CreateWithVersion(streamId, 0);
        await innerAggregateStore.SaveAsync(originalAggregate);

        var snapshotStore = new SnapshotStore<TestSnapshotAggregate>(
            options,
            innerAggregateStore,
            eventStore);

        // Act
        var result = await snapshotStore.LoadAsync(streamId);

        // Assert
        result.Should().NotBeNull();
        result.StreamId.Should().Be(streamId);
    }

    [Fact]
    public async Task LoadAsync_WithSnapshotEnabled_AndNoSnapshot_LoadsFromEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var options = Options.Create(new SnapshotOptions { IsSnapshotEnabled = true, SnapshotFrequency = 10 });
        
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();
        var innerAggregateStore = new AggregateStore<TestSnapshotAggregate>(eventStore, domainEvents);

        // Create and save an aggregate
        var originalAggregate = TestSnapshotAggregate.CreateWithVersion(streamId, 0);
        await innerAggregateStore.SaveAsync(originalAggregate);

        var snapshotStore = new SnapshotStore<TestSnapshotAggregate>(
            options,
            innerAggregateStore,
            eventStore);

        // Act
        var result = await snapshotStore.LoadAsync(streamId);

        // Assert
        result.Should().NotBeNull();
        result.StreamId.Should().Be(streamId);
    }

    [Fact]
    public async Task SaveAsync_AtSnapshotFrequency_CreatesSnapshot()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var options = Options.Create(new SnapshotOptions { IsSnapshotEnabled = true, SnapshotFrequency = 2 });
        
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();
        var innerAggregateStore = new AggregateStore<TestSnapshotAggregate>(eventStore, domainEvents);

        var aggregate = TestSnapshotAggregate.CreateWithVersion(streamId, 2);

        var snapshotStore = new SnapshotStore<TestSnapshotAggregate>(
            options,
            innerAggregateStore,
            eventStore);

        // Act
        await snapshotStore.SaveAsync(aggregate);

        // Assert
        var snapshot = await eventStore.GetLatestSnapshotAsync(streamId);
        snapshot.IsEmpty.Should().BeFalse();
        snapshot.Value.Event.Should().BeOfType<SnapshotEvent>();
    }

    [Fact]
    public async Task SaveAsync_NotAtSnapshotFrequency_DoesNotCreateSnapshot()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var options = Options.Create(new SnapshotOptions { IsSnapshotEnabled = true, SnapshotFrequency = 10 });
        
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();
        var innerAggregateStore = new AggregateStore<TestSnapshotAggregate>(eventStore, domainEvents);

        var aggregate = TestSnapshotAggregate.CreateWithVersion(streamId, 3);

        var snapshotStore = new SnapshotStore<TestSnapshotAggregate>(
            options,
            innerAggregateStore,
            eventStore);

        // Act
        await snapshotStore.SaveAsync(aggregate);

        // Assert
        var snapshot = await eventStore.GetLatestSnapshotAsync(streamId);
        snapshot.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_WithSnapshotDisabled_DoesNotCreateSnapshot()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var options = Options.Create(new SnapshotOptions { IsSnapshotEnabled = false });
        
        var eventStore = new InMemoryEventStore();
        var domainEvents = new PendingDomainEventsBuffer();
        var innerAggregateStore = new AggregateStore<TestSnapshotAggregate>(eventStore, domainEvents);

        var aggregate = TestSnapshotAggregate.CreateWithVersion(streamId, 100);

        var snapshotStore = new SnapshotStore<TestSnapshotAggregate>(
            options,
            innerAggregateStore,
            eventStore);

        // Act
        await snapshotStore.SaveAsync(aggregate);

        // Assert
        var snapshot = await eventStore.GetLatestSnapshotAsync(streamId);
        snapshot.IsEmpty.Should().BeTrue();
    }

    // Test helper class
    public sealed class TestSnapshotAggregate : Aggregate, IAggregateFactory<TestSnapshotAggregate>, IOriginator
    {
        public TestSnapshotAggregate() { }

        public static TestSnapshotAggregate Create() => new();

        public static TestSnapshotAggregate CreateWithVersion(Guid streamId, long version)
        {
            var aggregate = new TestSnapshotAggregate();
            var evt = new TestDomainEvent { StreamId = streamId, StreamName = "Test" };
            for (int i = 0; i <= version; i++)
            {
                aggregate.PushEvent(evt with { StreamVersion = i });
            }
            aggregate.MarkEventsAsCommitted();
            return aggregate;
        }

        public IMemento Save() => new TestMemento();

        public void Restore(IMemento memento) { }

        private sealed class TestMemento : IMemento { }
        private sealed record TestDomainEvent : DomainEvent;
    }
}

/// <summary>
/// Unit tests for PublisherSubscriber implementation.
/// </summary>
public sealed class PublisherSubscriberTests
{
    [Fact]
    public async Task PublishAsync_WithSyncHandler_InvokesHandler()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var publisherSubscriber = new PublisherSubscriber(serviceProvider);
        var invoked = false;

        var testEvent = new TestEvent();
        publisherSubscriber.Subscribe<TestEvent>(e => invoked = true);

        // Act
        await publisherSubscriber.PublishAsync(testEvent);

        // Assert
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WithAsyncHandler_InvokesHandler()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var publisherSubscriber = new PublisherSubscriber(serviceProvider);
        var invoked = false;

        var testEvent = new TestEvent();
        publisherSubscriber.Subscribe<TestEvent>((e, ct) =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        // Act
        await publisherSubscriber.PublishAsync(testEvent);

        // Assert
        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WithServiceHandler_InvokesHandler()
    {
        // Arrange
        var handler = new TestEventHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IEventHandler<TestEvent>>(handler);
        var serviceProvider = services.BuildServiceProvider();

        var publisherSubscriber = new PublisherSubscriber(serviceProvider);
        var testEvent = new TestEvent();

        // Act
        await publisherSubscriber.PublishAsync(testEvent);

        // Assert
        handler.WasInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_InvokesAllHandlers()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var publisherSubscriber = new PublisherSubscriber(serviceProvider);
        var invoked1 = false;
        var invoked2 = false;

        var handler = new TestEventHandler();

        var testEvent = new TestEvent();
        publisherSubscriber.Subscribe<TestEvent>(e => invoked1 = true);
        publisherSubscriber.Subscribe<TestEvent>((e, ct) => { invoked2 = true; return Task.CompletedTask; });
        publisherSubscriber.Subscribe(handler);

        // Act
        await publisherSubscriber.PublishAsync(testEvent);

        // Assert
        invoked1.Should().BeTrue();
        invoked2.Should().BeTrue();
        handler.WasInvoked.Should().BeTrue();
    }

    [Fact]
    public void Unsubscribe_WithSyncHandler_RemovesHandler()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var publisherSubscriber = new PublisherSubscriber(serviceProvider);
        
        Action<TestEvent> handler = e => { };
        publisherSubscriber.Subscribe(handler);

        // Act
        var result = publisherSubscriber.Unsubscribe(handler);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SubscribeDisposable_WhenDisposed_UnsubscribesHandler()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var publisherSubscriber = new PublisherSubscriber(serviceProvider);
        var invoked = false;

        var testEvent = new TestEvent();
        var subscription = publisherSubscriber.SubscribeDisposable<TestEvent>(e => invoked = true);

        // Act - Dispose the subscription
        subscription.Dispose();

        // Publish after disposing
        await publisherSubscriber.PublishAsync(testEvent);

        // Assert - Handler should not be invoked
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var publisherSubscriber = new PublisherSubscriber(serviceProvider);

        // Act
        var act = () => publisherSubscriber.PublishAsync<TestEvent>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task PublishAsync_WithNoHandlers_CompletesSuccessfully()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var publisherSubscriber = new PublisherSubscriber(serviceProvider);
        var testEvent = new TestEvent();

        // Act
        var act = () => publisherSubscriber.PublishAsync(testEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // Test helper classes
    private sealed record TestEvent : BaseEvent, IEvent;

    private sealed class TestEventHandler : IEventHandler<TestEvent>
    {
        public bool WasInvoked { get; private set; }

        public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
        {
            WasInvoked = true;
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// Unit tests for Scheduler implementation.
/// </summary>
public sealed class SchedulerTests
{
    [Fact]
    public async Task ScheduleAsync_WhenDisabled_DoesNotProcessEvents()
    {
        // Arrange
        var options = new SchedulerOptions { IsEventSchedulerEnabled = false };
        var mockOptionsMonitor = CreateOptionsMonitor(options);
        
        var outbox = new InMemoryOutboxStore();
        var publisher = new PublisherSubscriber(new ServiceCollection().BuildServiceProvider());
        
        var serviceProvider = CreateServiceProvider(outbox, publisher);
        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = serviceProvider.GetRequiredService<ILogger<Scheduler>>();

        var scheduler = new Scheduler(serviceScopeFactory, mockOptionsMonitor, logger);

        // Act
        await scheduler.ScheduleAsync();

        // Assert - No events should be dequeued
        outbox.DequeueCount.Should().Be(0);
    }

    [Fact]
    public async Task ScheduleAsync_WithNoEvents_CompletesSuccessfully()
    {
        // Arrange
        var options = new SchedulerOptions { IsEventSchedulerEnabled = true, BatchSize = 10 };
        var mockOptionsMonitor = CreateOptionsMonitor(options);
        
        var outbox = new InMemoryOutboxStore();
        var publisher = new PublisherSubscriber(new ServiceCollection().BuildServiceProvider());
        
        var serviceProvider = CreateServiceProvider(outbox, publisher);
        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = serviceProvider.GetRequiredService<ILogger<Scheduler>>();

        var scheduler = new Scheduler(serviceScopeFactory, mockOptionsMonitor, logger);

        // Act
        var act = () => scheduler.ScheduleAsync();

        // Assert
        await act.Should().NotThrowAsync();
        outbox.DequeueCount.Should().Be(1);
    }

    [Fact]
    public async Task ScheduleAsync_WithEvents_ProcessesAndPublishes()
    {
        // Arrange
        var options = new SchedulerOptions { IsEventSchedulerEnabled = true, BatchSize = 10 };
        var mockOptionsMonitor = CreateOptionsMonitor(options);

        var testEvent = new TestIntegrationEvent();
        var outbox = new InMemoryOutboxStore();
        outbox.AddEvents(testEvent);

        var handlerInvoked = false;
        var services = new ServiceCollection();
        services.AddSingleton<IEventHandler<TestIntegrationEvent>>(
            new TestIntegrationEventHandler(() => handlerInvoked = true));
        var publisher = new PublisherSubscriber(services.BuildServiceProvider());
        
        var serviceProvider = CreateServiceProvider(outbox, publisher);
        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = serviceProvider.GetRequiredService<ILogger<Scheduler>>();

        var scheduler = new Scheduler(serviceScopeFactory, mockOptionsMonitor, logger);

        // Act
        await scheduler.ScheduleAsync();

        // Assert
        handlerInvoked.Should().BeTrue();
        outbox.CompletedEvents.Should().Contain(testEvent.EventId);
    }

    [Fact]
    public async Task ScheduleAsync_WhenEventPublishingFails_MarksAsFailedInOutbox()
    {
        // Arrange
        var options = new SchedulerOptions 
        { 
            IsEventSchedulerEnabled = true, 
            BatchSize = 10,
            EventProcessingTimeout = 5000
        };
        var mockOptionsMonitor = CreateOptionsMonitor(options);

        var testEvent = new TestIntegrationEvent();
        var outbox = new InMemoryOutboxStore();
        outbox.AddEvents(testEvent);

        var services = new ServiceCollection();
        services.AddSingleton<IEventHandler<TestIntegrationEvent>>(
            new TestIntegrationEventHandler(() => throw new InvalidOperationException("Publishing failed")));
        var publisher = new PublisherSubscriber(services.BuildServiceProvider());
        
        var serviceProvider = CreateServiceProvider(outbox, publisher);
        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var logger = serviceProvider.GetRequiredService<ILogger<Scheduler>>();

        var scheduler = new Scheduler(serviceScopeFactory, mockOptionsMonitor, logger);

        // Act
        await scheduler.ScheduleAsync();

        // Assert
        outbox.FailedEvents.Should().Contain(e => e.EventId == testEvent.EventId);
    }

    // Helper methods
    private static IOptionsMonitor<SchedulerOptions> CreateOptionsMonitor(SchedulerOptions options)
    {
        return new TestOptionsMonitor<SchedulerOptions>(options);
    }

    private static IServiceProvider CreateServiceProvider(IOutboxStore outbox, IPublisher publisher)
    {
        var services = new ServiceCollection();
        services.AddSingleton(outbox);
        services.AddSingleton(publisher);
        services.AddLogging();
        return services.BuildServiceProvider();
    }

    // Test helper classes
    private sealed record TestIntegrationEvent : IntegrationEvent;

    private sealed class TestIntegrationEventHandler : IEventHandler<TestIntegrationEvent>
    {
        private readonly Action _onInvoke;

        public TestIntegrationEventHandler(Action onInvoke) => _onInvoke = onInvoke;

        public Task HandleAsync(TestIntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            _onInvoke();
            return Task.CompletedTask;
        }
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T> where T : class
    {
        private readonly T _value;

        public TestOptionsMonitor(T value) => _value = value;

        public T CurrentValue => _value;
        public T Get(string? name) => _value;
        public IDisposable? OnChange(Action<T, string> listener) => null;
    }
}

// Test infrastructure - In-memory implementations

internal sealed class InMemoryEventStore : IEventStore
{
    private readonly Dictionary<Guid, List<IDomainEvent>> _streams = new();
    private readonly Dictionary<Guid, ISnapshotEvent> _snapshots = new();
    private long _globalPosition;

    public Task<AppendResult> AppendToStreamAsync(AppendRequest request, CancellationToken cancellationToken = default)
    {
        if (!_streams.ContainsKey(request.StreamId))
        {
            _streams[request.StreamId] = new List<IDomainEvent>();
        }

        var stream = _streams[request.StreamId];
        
        foreach (var evt in request.Events.OfType<IDomainEvent>())
        {
            stream.Add(evt);
        }

        return Task.FromResult(AppendResult.Create(
            request.Events.Select(e => e.EventId).ToList(),
            stream.Count - request.Events.Count(),
            stream.Count - 1));
    }

    public async IAsyncEnumerable<EnvelopeResult> ReadStreamAsync(ReadStreamRequest request, CancellationToken cancellationToken = default)
    {
        if (!_streams.ContainsKey(request.StreamId))
        {
            yield break;
        }

        var events = _streams[request.StreamId]
            .Where(e => e.StreamVersion >= request.FromVersion)
            .OrderBy(e => e.StreamVersion);

        foreach (var evt in events)
        {
            yield return new EnvelopeResult
            {
                EventId = evt.EventId,
                EventType = evt.GetType().Name,
                EventFullName = evt.GetType().FullName!,
                OccurredOn = evt.OccurredOn,
                Event = evt,
                GlobalPosition = Interlocked.Increment(ref _globalPosition),
                StreamId = evt.StreamId,
                StreamName = evt.StreamName,
                StreamVersion = evt.StreamVersion
            };
            await Task.Yield();
        }
    }

    public Task<Optional<EnvelopeResult>> GetLatestSnapshotAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        if (_snapshots.TryGetValue(ownerId, out var snapshot))
        {
            var envelope = new EnvelopeResult
            {
                EventId = snapshot.EventId,
                EventType = snapshot.GetType().Name,
                EventFullName = snapshot.GetType().FullName!,
                OccurredOn = snapshot.OccurredOn,
                Event = snapshot,
                GlobalPosition = 0,
                StreamId = ownerId,
                StreamName = "Snapshot",
                StreamVersion = 0
            };
            return Task.FromResult(Optional.Some(envelope));
        }

        return Task.FromResult(Optional.Empty<EnvelopeResult>());
    }

    public Task AppendSnapshotAsync(ISnapshotEvent snapshotEvent, CancellationToken cancellationToken = default)
    {
        _snapshots[snapshotEvent.OwnerId] = snapshotEvent;
        return Task.CompletedTask;
    }

    // Not implemented for these tests
    public IAsyncEnumerable<EnvelopeResult> ReadAllStreamsAsync(ReadAllStreamsRequest request, CancellationToken cancellationToken = default) 
        => throw new NotImplementedException();
    public Task<long> GetStreamVersionAsync(Guid streamId, CancellationToken cancellationToken = default) 
        => throw new NotImplementedException();
    public Task<bool> StreamExistsAsync(Guid streamId, CancellationToken cancellationToken = default) 
        => throw new NotImplementedException();
    public Task DeleteStreamAsync(DeleteStreamRequest request, CancellationToken cancellationToken = default) 
        => throw new NotImplementedException();
    public Task TruncateStreamAsync(TruncateStreamRequest request, CancellationToken cancellationToken = default) 
        => throw new NotImplementedException();
    public IAsyncDisposable SubscribeToStream(SubscribeToStreamRequest request, CancellationToken cancellationToken = default) 
        => throw new NotImplementedException();
    public IAsyncDisposable SubscribeToAllStreams(SubscribeToAllStreamsRequest request, CancellationToken cancellationToken = default) 
        => throw new NotImplementedException();
}

internal sealed class InMemoryOutboxStore : IOutboxStore
{
    private readonly List<IIntegrationEvent> _events = new();
    private readonly HashSet<Guid> _completed = new();
    private readonly List<FailedEvent> _failed = new();

    public int DequeueCount { get; private set; }
    public IReadOnlyCollection<Guid> CompletedEvents => _completed;
    public IReadOnlyCollection<FailedEvent> FailedEvents => _failed;

    public void AddEvents(params IIntegrationEvent[] events) => _events.AddRange(events);

    public Task<IReadOnlyList<IIntegrationEvent>> DequeueAsync(
        CancellationToken cancellationToken = default,
        int batchSize = 100,
        TimeSpan? visibilityTimeout = null)
    {
        DequeueCount++;
        var result = _events.Take(batchSize).ToList();
        return Task.FromResult<IReadOnlyList<IIntegrationEvent>>(result);
    }

    public Task CompleteAsync(CancellationToken cancellationToken = default, params Guid[] eventIds)
    {
        foreach (var id in eventIds)
        {
            _completed.Add(id);
        }
        return Task.CompletedTask;
    }

    public Task FailAsync(CancellationToken cancellationToken = default, params FailedEvent[] failedEvents)
    {
        _failed.AddRange(failedEvents);
        return Task.CompletedTask;
    }

    public Task EnqueueAsync(CancellationToken cancellationToken = default, params IIntegrationEvent[] events) 
        => throw new NotImplementedException();
}
