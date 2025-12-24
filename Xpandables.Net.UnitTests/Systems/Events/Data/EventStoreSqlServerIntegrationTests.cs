using System.DependencyInjection;
using System.Events.Aggregates;
using System.Events.Data;
using System.Events.Data.Configurations;
using System.Events.Domain;
using System.Text.Json;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Systems.Events.Data;

public sealed class EventStoreSqlServerIntegrationTests
{
    [Fact]
    public async Task EventStore_WithLocalDb_AttachesAndRehydratesAggregates()
    {
        var database = new LocalDbDatabaseFixture();
        using ServiceProvider provider = BuildSqlServerProvider(database);

        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        var eventDb = scope.ServiceProvider.GetRequiredService<EventStoreDataContext>();
        var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxStoreDataContext>();
        var readStoreDb = scope.ServiceProvider.GetRequiredService<ReadStoreProbeContext>();

        var aggregateStore = scope.ServiceProvider.GetRequiredService<IAggregateStore<TestBankAccountAggregate>>();
        var pendingBuffer = scope.ServiceProvider.GetRequiredService<IPendingDomainEventsBuffer>();
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        Guid streamId = Guid.NewGuid();
        var aggregate = TestBankAccountAggregate.Initialize();
        aggregate.Open(streamId, "Ada", 125m);
        aggregate.Deposit(25m);

        await aggregateStore.SaveAsync(aggregate);
        var batches = pendingBuffer.Drain();
        await eventStore.FlushEventsAsync();
        foreach (var batch in batches)
        {
            batch.OnCommitted();
        }

        var reloaded = await aggregateStore.LoadAsync(streamId);
        reloaded.Balance.Should().Be(150m);
        reloaded.StreamVersion.Should().Be(1);
        (await eventStore.StreamExistsAsync(streamId)).Should().BeTrue();

        var envelopes = new List<EnvelopeResult>();
        await foreach (var envelope in eventStore.ReadStreamAsync(new ReadStreamRequest
        {
            StreamId = streamId,
            FromVersion = -1,
            MaxCount = 10
        }))
        {
            envelopes.Add(envelope);
        }

        envelopes.Should().HaveCount(2);
    }

    private static ServiceProvider BuildSqlServerProvider(LocalDbDatabaseFixture database)
    {
        var services = new ServiceCollection();
        IConfiguration configuration = StaticConfiguration.Configuration;
        services.AddLogging();
        services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        services.AddXCacheTypeResolver(typeof(TestBankAccountAggregate).Assembly, typeof(AccountOpened).Assembly);
        services.AddXEventConverterFactory();
        services.AddXEventStoreDataContext(options =>
            options.UseSqlServer(configuration.GetConnectionString(database.EventStoreConnectionString))
            .ReplaceService<IModelCustomizer, EventStoreSqlServerModelCustomizer>());
        services.AddXOutboxStoreDataContext(options =>
            options.UseSqlServer(configuration.GetConnectionString(database.EventStoreConnectionString))
            .ReplaceService<IModelCustomizer, OutboxStoreSqlServerModelCustomizer>());
        services.AddDbContext<ReadStoreProbeContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(database.ReadStoreConnectionString)));
        services.AddXEventStore();
        services.AddXOutboxStore();
        services.AddXJsonSerializerOptions();
        services.AddXEventConverterFactory();
        services.AddXEventConverterContext();
        services.AddScoped<IPendingDomainEventsBuffer, PendingDomainEventsBuffer>();
        services.AddXAggregateStore<TestBankAccountAggregate>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    private sealed class LocalDbDatabaseFixture
    {
        public LocalDbDatabaseFixture()
        {
            EventStoreConnectionString = "EventStoreDb";
            ReadStoreConnectionString = "ReadStoreDb";
        }

        public string EventStoreConnectionString { get; }
        public string ReadStoreConnectionString { get; }
    }

    private sealed class ReadStoreProbeContext(DbContextOptions<ReadStoreProbeContext> options) : DbContext(options)
    {
    }
}
