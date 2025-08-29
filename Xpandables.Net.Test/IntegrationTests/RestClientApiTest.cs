using System.Net;
using System.Net.Http.Headers;
using System.Text;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;
using Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;
using Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events;
using Xpandables.Net.Repositories;
using Xpandables.Net.Rests;

namespace Xpandables.Net.Test.IntegrationTests;

[TestCaseOrderer("Xpandables.Net.Test.PriorityOrderer", "Xpandables.Net.Test")]
public sealed class RestClientApiTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IRestClient _restClient;
    private readonly Guid _keyId = Guid.CreateVersion7();

    public RestClientApiTest(WebApplicationFactory<Program> factory)
    {
        // Replace the event store in the SUT with an in-memory implementation
        factory = factory.WithWebHostBuilder(builder => builder.ConfigureServices(
            services => services.Replace(
                new ServiceDescriptor(typeof(IEventStore),
                typeof(InMemoryEventStore),
                ServiceLifetime.Scoped))));

        var services = new ServiceCollection();
        services.AddXRestAttributeProvider();
        services.AddXRestRequestBuilders();
        services.AddXRestResponseBuilders();
        services.AddXRestRequestHandler();
        services.AddXRestResponseHandler();

        // HttpClient sourced from the test server
        services.AddSingleton(factory.CreateClient());
        services.AddScoped<IRestClient>(provider =>
        {
            var httpClient = provider.GetRequiredService<HttpClient>();
            return new RestClient(provider, httpClient);
        });

        _restClient = services.BuildServiceProvider().GetRequiredService<IRestClient>();
    }

    [Fact, TestPriority(0)]
    public async Task CreateAccount_Should_Return_Valid_Result()
    {
        // Upload picture (in-memory multipart)
        var uploadPictureRequest = new UploadPictureRequest();
        var uploadPictureResponse = await _restClient.SendAsync(uploadPictureRequest);
        uploadPictureResponse.IsSuccess.Should().BeTrue();

        // Create
        var create = new CreateAccountRequest { KeyId = _keyId };
        var createResponse = await _restClient.SendAsync(create);
        createResponse.IsSuccess.Should().BeTrue();
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Deposit
        var deposit = new DepositAccountRequest { KeyId = _keyId, Amount = 100 };
        var depositResponse = await _restClient.SendAsync(deposit);
        depositResponse.IsSuccess.Should().BeTrue();
        depositResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Balance
        var balance = new GetBalanceAccountRequest { KeyId = _keyId };
        var balanceResponse = await _restClient.SendAsync(balance);
        balanceResponse.IsSuccess.Should().BeTrue();
        balanceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        balanceResponse.Result.Should().Be(100);
    }

    // In-memory multipart to avoid file-system dependency
    [RestPut("/accounts/picture",
        IsSecured = false,
        ContentType = Rest.ContentType.MultipartFormData,
        Location = Rest.Location.Body,
        BodyFormat = Rest.BodyFormat.Multipart)]
    public sealed record UploadPictureRequest : IRestRequest, IRestMultipart
    {
        public MultipartFormDataContent GetMultipartContent()
        {
            var multipart = new MultipartFormDataContent("boundary");
            var bytes = Encoding.UTF8.GetBytes("fake image content");
            var stream = new MemoryStream(bytes);
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            multipart.Add(fileContent, "formFile", "Signature.png");
            return multipart;
        }
    }
}

/// <summary>
/// Minimal in-memory event store for integration tests, compatible with the new stream-first API.
/// Stores domain events per aggregate with optimistic concurrency.
/// </summary>
internal sealed class InMemoryEventStore : IEventStore
{
    private readonly object _lock = new();
    private readonly static Dictionary<Guid, List<(long Version, IDomainEvent Event, DateTimeOffset At)>> _streams = [];
    private readonly List<(long Position, IEvent Event, Guid? AggregateId, long? StreamVersion, string? AggregateName, DateTimeOffset At)> _global = [];
    private readonly static Dictionary<Guid, ISnapshotEvent> _snapshots = [];
    private long _position;

    public Task<AppendResult> AppendToStreamAsync(
        Guid aggregateId,
        string aggregateName,
        long expectedVersion,
        IEnumerable<IDomainEvent> events,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);
        var batch = events as IDomainEvent[] ?? [.. events];
        if (batch.Length == 0)
            return Task.FromResult(AppendResult.Create(expectedVersion + 1, expectedVersion));

        lock (_lock)
        {
            if (!_streams.TryGetValue(aggregateId, out var list))
            {
                list = [];
                _streams[aggregateId] = list;
            }

            var isNewStream = list.Count == 0;
            var current = isNewStream ? -1 : list[^1].Version;

            // Accept both -1 and 0 as "no stream yet" for tests
            if (isNewStream && expectedVersion == 0)
                expectedVersion = -1;

            if (current != expectedVersion)
                throw new InvalidOperationException($"Concurrency violation for {aggregateId}. Expected {expectedVersion}, found {current}.");

            var first = expectedVersion + 1;
            var ids = new List<Guid>(batch.Length);

            foreach (var e in batch)
            {
                var nextVersion = ++current;
                var stamped = e
                    .WithAggregateId(aggregateId)
                    .WithStreamVersion(nextVersion)
                    .WithAggregateName(aggregateName);

                list.Add((nextVersion, stamped, DateTimeOffset.UtcNow));

                var pos = ++_position;
                _global.Add((pos, stamped, aggregateId, nextVersion, aggregateName, DateTimeOffset.UtcNow));
                ids.Add(stamped.Id);
            }

            return Task.FromResult(AppendResult.Create(ids.ToArray(), first, current));
        }
    }

    public async IAsyncEnumerable<EventEnvelope> ReadStreamAsync(
           Guid aggregateId,
           long fromVersion = -1,
           int maxCount = int.MaxValue,
           [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        KeyValuePair<Guid, List<(long Version, IDomainEvent Event, DateTimeOffset At)>>? kvp;
        lock (_lock)
        {
            kvp = _streams.TryGetValue(aggregateId, out var list)
                ? new KeyValuePair<Guid, List<(long, IDomainEvent, DateTimeOffset)>>(aggregateId, list)
                : null;
        }

        if (kvp is null) yield break;

        foreach (var (ver, ev, at) in kvp.Value.Value
                     .Where(i => i.Version > fromVersion) // exclusive lower bound
                     .OrderBy(i => i.Version)
                     .Take(maxCount))
        {
            yield return new EventEnvelope
            {
                EventId = ev.Id,
                EventType = ev.GetType().Name,
                EventFullName = ev.GetType().FullName!,
                OccurredOn = at,
                Event = ev,
                GlobalPosition = 0,
                AggregateId = aggregateId,
                AggregateName = ev.AggregateName,
                StreamVersion = ver
            };
            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<EventEnvelope> ReadAllAsync(
        long fromPosition = 0,
        int maxCount = 4096,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<(long Position, IEvent Event, Guid? AggregateId, long? StreamVersion, string? AggregateName, DateTimeOffset At)> items;
        lock (_lock)
        {
            items = _global.Where(g => g.Position > fromPosition).OrderBy(g => g.Position).Take(maxCount).ToList();
        }

        foreach (var g in items)
        {
            yield return new EventEnvelope
            {
                EventId = (g.Event as IDomainEvent)?.Id ?? Guid.CreateVersion7(),
                EventType = g.Event.GetType().Name,
                EventFullName = g.Event.GetType().FullName!,
                OccurredOn = g.At,
                Event = g.Event,
                GlobalPosition = g.Position,
                AggregateId = g.AggregateId,
                AggregateName = g.AggregateName,
                StreamVersion = g.StreamVersion
            };
            await Task.Yield();
        }
    }

    public Task<long> GetStreamVersionAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_streams.TryGetValue(aggregateId, out var list) || list.Count == 0) return Task.FromResult(-1L);
            return Task.FromResult(list[^1].Version);
        }
    }

    public Task AppendSnapshotAsync(Guid aggregateId, ISnapshotEvent snapshot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        lock (_lock)
        {
            _snapshots[aggregateId] = snapshot;
        }
        return Task.CompletedTask;
    }

    public Task<EventEnvelope?> ReadLatestSnapshotAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_snapshots.TryGetValue(aggregateId, out var snap))
                return Task.FromResult<EventEnvelope?>(null);

            var env = new EventEnvelope
            {
                EventId = snap.Id,
                EventType = snap.GetType().Name,
                EventFullName = snap.GetType().FullName!,
                OccurredOn = DateTimeOffset.UtcNow,
                Event = snap,
                GlobalPosition = 0,
                AggregateId = aggregateId,
                AggregateName = null,
                StreamVersion = null
            };

            return Task.FromResult<EventEnvelope?>(env);
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}