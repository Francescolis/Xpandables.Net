using System.Events;
using System.Events.Aggregates;
using System.Events.Domain;
using System.Runtime.CompilerServices;

namespace Xpandables.Net.UnitTests.Systems.Events;

internal sealed class TestBankAccountAggregate : Aggregate, IAggregateFactory<TestBankAccountAggregate>
{
    public decimal Balance { get; private set; }
    public string OwnerName { get; private set; } = string.Empty;

    private TestBankAccountAggregate()
    {
        On<AccountOpened>(Apply);
        On<MoneyDeposited>(Apply);
    }

    public static TestBankAccountAggregate Initialize() => new();

    public void Open(Guid accountId, string ownerName, decimal openingBalance)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(accountId, Guid.Empty);
        if (!IsEmpty)
        {
            throw new InvalidOperationException("The account is already opened.");
        }

        var @event = new AccountOpened
        {
            StreamId = accountId,
            OwnerName = ownerName,
            Amount = openingBalance
        };

        AppendEvent(@event);
    }

    public void Deposit(decimal amount)
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("Open the account before depositing funds.");
        }

        var @event = new MoneyDeposited
        {
            StreamId = StreamId,
            Amount = amount
        };

        AppendEvent(@event);
    }

    private void Apply(AccountOpened @event)
    {
        OwnerName = @event.OwnerName;
        Balance = @event.Amount;
    }

    private void Apply(MoneyDeposited @event) => Balance += @event.Amount;
}

public sealed record AccountOpened : DomainEvent
{
    public required string OwnerName { get; init; }
    public required decimal Amount { get; init; }
}

public sealed record MoneyDeposited : DomainEvent
{
    public required decimal Amount { get; init; }
}

internal sealed class TestMoneyDepositedHandler : IEventHandler<MoneyDeposited>
{
    private readonly List<MoneyDeposited> _handled = [];

    public IReadOnlyCollection<MoneyDeposited> HandledEvents => _handled;

    public Task HandleAsync(MoneyDeposited eventInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventInstance);
        _handled.Add(eventInstance);
        return Task.CompletedTask;
    }
}

internal sealed class FakeEventStore : IDomainStore
{
    private readonly List<IDomainEvent> _events = [];
    private readonly AsyncDisposable _subscription = new();

    public AppendRequest? LastAppendRequest { get; private set; }

    public FakeEventStore(IEnumerable<IDomainEvent>? history = null)
    {
        if (history is null)
        {
            return;
        }

        foreach (var domainEvent in history)
        {
            _events.Add(domainEvent);
        }
    }

    public Task<AppendResult> AppendToStreamAsync(AppendRequest request, CancellationToken cancellationToken = default)
    {
        var batch = request.Events.OfType<IDomainEvent>().ToArray();
        LastAppendRequest = new AppendRequest
        {
            StreamId = request.StreamId,
            Events = batch,
            ExpectedVersion = request.ExpectedVersion
        };

        if (request.ExpectedVersion.HasValue)
        {
            long current = GetStreamVersionInternal(request.StreamId);
            if (current != request.ExpectedVersion.Value)
            {
                throw new InvalidOperationException(
                    $"Concurrency violation for {request.StreamId}. Expected {request.ExpectedVersion.Value} but found {current}.");
            }
        }

        foreach (var domainEvent in batch)
        {
            _events.Add(domainEvent);
        }

        if (batch.Length == 0)
        {
            return Task.FromResult(AppendResult.Empty);
        }

        long first = batch.First().StreamVersion;
        long last = batch.Last().StreamVersion;
        return Task.FromResult(AppendResult.Create(
            [.. batch.Select(@event => @event.EventId)], first, last));
    }

    public async IAsyncEnumerable<EnvelopeResult> ReadStreamAsync(
        ReadStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var ordered = _events
            .Where(@event => @event.StreamId == request.StreamId && @event.StreamVersion > request.FromVersion)
            .OrderBy(@event => @event.StreamVersion)
            .ToArray();

        foreach (var domainEvent in ordered)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return new EnvelopeResult
            {
                Event = domainEvent,
                EventId = domainEvent.EventId,
                EventName = domainEvent.GetEventName(),
                OccurredOn = domainEvent.OccurredOn,
                GlobalPosition = domainEvent.StreamVersion,
                StreamId = domainEvent.StreamId,
                StreamName = domainEvent.StreamName,
                StreamVersion = domainEvent.StreamVersion
            };
        }

        await Task.CompletedTask;
    }

    public Task<long> GetStreamVersionAsync(Guid streamId, CancellationToken cancellationToken = default) =>
        Task.FromResult(GetStreamVersionInternal(streamId));

    public Task<bool> StreamExistsAsync(Guid streamId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_events.Any(@event => @event.StreamId == streamId));

    public Task DeleteStreamAsync(DeleteStreamRequest request, CancellationToken cancellationToken = default)
    {
        _events.RemoveAll(@event => @event.StreamId == request.StreamId);
        return Task.CompletedTask;
    }

    public Task TruncateStreamAsync(TruncateStreamRequest request, CancellationToken cancellationToken = default)
    {
        _events.RemoveAll(@event =>
            @event.StreamId == request.StreamId && @event.StreamVersion < request.TruncateBeforeVersion);
        return Task.CompletedTask;
    }

    public Task AppendSnapshotAsync(ISnapshotEvent snapshotEvent, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<EnvelopeResult?> GetLatestSnapshotAsync(Guid ownerId, CancellationToken cancellationToken = default) =>
        Task.FromResult<EnvelopeResult?>(null);

    public IAsyncEnumerable<EnvelopeResult> ReadAllStreamsAsync(
        ReadAllStreamsRequest request,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public IAsyncDisposable SubscribeToStream(
        SubscribeToStreamRequest request,
        CancellationToken cancellationToken = default) =>
        _subscription;

    public IAsyncDisposable SubscribeToAllStreams(
        SubscribeToAllStreamsRequest request,
        CancellationToken cancellationToken = default) =>
        _subscription;

    public Task FlushEventsAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    private long GetStreamVersionInternal(Guid streamId)
    {
        var last = _events
            .Where(@event => @event.StreamId == streamId)
            .OrderByDescending(@event => @event.StreamVersion)
            .FirstOrDefault();

        return last?.StreamVersion ?? -1;
    }

    private sealed class AsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
