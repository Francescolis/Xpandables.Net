using System.Collections.Concurrent;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Test.UnitTests;

public sealed class EventSchedulerUnitTest
{
    private readonly IServiceProvider _serviceProvider;

    public EventSchedulerUnitTest()
    {
        var services = new ServiceCollection();

        // Scheduler + publisher
        services.AddLogging();
        services.AddXPublisher();
        services.AddXSubscriber();
        services.AddXScheduler();

        // Outbox store (in-memory for tests)
        services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();

        // Register a handler for our test integration event to assert publishing
        services.AddXEventHandler<TestIntegrationEvent, TestIntegrationEventHandler>(factory: null);

        // Scheduler options (ensure enabled)
        services.Configure<SchedulerOptions>(opt =>
        {
            opt.IsEventSchedulerEnabled = true;
            opt.BatchSize = 32;
            opt.MaxConcurrentProcessors = 2;
            opt.SchedulerFrequency = 25;
            opt.EventProcessingTimeout = 3000;
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Scheduler_ShouldPublish_And_CompleteOutbox()
    {
        // Arrange
        var outbox = _serviceProvider.GetRequiredService<IOutboxStore>();
        var scheduler = _serviceProvider.GetRequiredService<IScheduler>();
        var optionsMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<SchedulerOptions>>();

        // Safety: ensure scheduler runs once for this test
        optionsMonitor.CurrentValue.IsEventSchedulerEnabled.Should().BeTrue();

        var testEvent = new TestIntegrationEvent { Id = Guid.CreateVersion7() };

        await outbox.EnqueueAsync(testEvent, CancellationToken.None);

        // Act
        await scheduler.ScheduleAsync(CancellationToken.None);

        // Assert: handler received the event
        TestIntegrationEventHandler.PublishedIds.Should().Contain(testEvent.Id);

        // Assert: outbox marked completion
        var inMemory = (InMemoryOutboxStore)_serviceProvider.GetRequiredService<IOutboxStore>();
        inMemory.GetCompleted().Should().Contain(testEvent.Id);
        inMemory.GetFailed().Should().BeEmpty();
    }
}

sealed record TestIntegrationEvent : IntegrationEvent { }
internal sealed class TestIntegrationEventHandler : IEventHandler<TestIntegrationEvent>
{
    public static ConcurrentBag<Guid> PublishedIds { get; } = new();

    public Task HandleAsync(TestIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        PublishedIds.Add(@event.Id);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryOutboxStore : IOutboxStore
{
    private readonly ConcurrentDictionary<Guid, Stored> _store = new();

    public Task EnqueueAsync(IIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _store[@event.Id] = new Stored(@event, OutboxStatus.Pending, Attempt: 0, NextAttemptOn: null, ClaimId: null);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IIntegrationEvent>> ClaimPendingAsync(
        int batchSize, TimeSpan? leaseDuration = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize);

        var now = DateTime.UtcNow;
        var lease = leaseDuration ?? TimeSpan.FromMinutes(5);
        var claimId = Guid.NewGuid();

        var candidates = _store.Values
            .Where(s =>
                s.Status is OutboxStatus.Pending or OutboxStatus.OnError &&
                (s.NextAttemptOn is null || s.NextAttemptOn <= now) &&
                s.ClaimId == null)
            .OrderBy(s => s.Event.OccurredOn)
            .Take(batchSize)
            .ToList();

        foreach (var s in candidates)
        {
            _store.AddOrUpdate(
                s.Event.Id,
                s,
                (_, old) => old with
                {
                    Status = OutboxStatus.Processing,
                    ClaimId = claimId,
                    NextAttemptOn = now.Add(lease)
                });
        }

        var claimed = _store.Values
            .Where(s => s.ClaimId == claimId)
            .OrderBy(s => s.Event.OccurredOn)
            .Select(s => s.Event)
            .ToList();

        return Task.FromResult((IReadOnlyList<IIntegrationEvent>)claimed);
    }

    public Task CompleteAsync(IEnumerable<Guid> eventIds, CancellationToken cancellationToken = default)
    {
        foreach (var id in eventIds)
        {
            if (_store.TryGetValue(id, out var s))
            {
                _store[id] = s with
                {
                    Status = OutboxStatus.Published,
                    ClaimId = null,
                    NextAttemptOn = null
                };
            }
        }
        return Task.CompletedTask;
    }

    public Task FailAsync(IEnumerable<(Guid EventId, string Error)> failures, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var (id, _) in failures)
        {
            if (_store.TryGetValue(id, out var s))
            {
                var attempt = s.Attempt + 1;
                _store[id] = s with
                {
                    Status = OutboxStatus.OnError,
                    Attempt = attempt,
                    ClaimId = null,
                    NextAttemptOn = Backoff(now, attempt)
                };
            }
        }
        return Task.CompletedTask;
    }

    // Helpers for assertions
    public IReadOnlyCollection<Guid> GetCompleted() =>
        _store.Values.Where(s => s.Status == OutboxStatus.Published).Select(s => s.Event.Id).ToArray();

    public IReadOnlyCollection<Guid> GetFailed() =>
        _store.Values.Where(s => s.Status == OutboxStatus.OnError).Select(s => s.Event.Id).ToArray();

    private static DateTime Backoff(DateTime now, int attempt)
    {
        var delay = TimeSpan.FromSeconds(Math.Min(600, 10 * Math.Pow(2, Math.Min(10, attempt - 1))));
        return now.Add(delay);
    }

    private enum OutboxStatus { Pending, Processing, OnError, Published }

    private readonly record struct Stored(
        IIntegrationEvent Event,
        OutboxStatus Status,
        int Attempt,
        DateTime? NextAttemptOn,
        Guid? ClaimId);
}