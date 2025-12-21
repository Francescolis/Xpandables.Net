/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Events;
using System.Events.Aggregates;
using System.Events.Data;
using System.Events.Data.Configurations;
using System.Events.Domain;
using System.Text.Json;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Systems.Events;

public sealed class EventServiceCollectionExtensionsTests
{
    [Fact]
    public void ServiceCollectionExtensions_RegisterEventInfrastructure()
    {
        // Arrange
        using var sqliteDirectory = new SqliteDatabaseDirectory();
        using ServiceProvider provider = BuildServiceProvider(sqliteDirectory);

        // Act
        using IServiceScope scope = provider.CreateScope();
        EventStoreDataContext eventDb = scope.ServiceProvider.GetRequiredService<EventStoreDataContext>();
        OutboxStoreDataContext outboxDb = scope.ServiceProvider.GetRequiredService<OutboxStoreDataContext>();
        eventDb.Database.EnsureCreated();
        outboxDb.Database.EnsureCreated();

        // Assert
        var aggregateStore = scope.ServiceProvider.GetRequiredService<IAggregateStore<TestBankAccountAggregate>>();
        aggregateStore.Should().NotBeNull();

        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var subscriber = scope.ServiceProvider.GetRequiredService<IEventSubscriber>();
        publisher.Should().BeSameAs(subscriber);

        scope.ServiceProvider.GetServices<IEventHandler<MoneyDeposited>>().Should().ContainSingle();
        scope.ServiceProvider.GetServices<IEventHandlerWrapper>().Should().NotBeEmpty();
    }

    private static ServiceProvider BuildServiceProvider(SqliteDatabaseDirectory directory)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        services.AddXCacheTypeResolver(typeof(TestBankAccountAggregate).Assembly);
        services.AddXEventConverterFactory();
        services.AddXEventStoreDataContext(options =>
            options.UseSqlite(directory.EventStoreConnectionString)
                    .ReplaceService<IModelCustomizer, EventStoreSqlServerModelCustomizer>());
        services.AddXOutboxStoreDataContext(options =>
            options.UseSqlite(directory.OutboxConnectionString)
                    .ReplaceService<IModelCustomizer, OutboxStoreSqlServerModelCustomizer>());
        services.AddXEventStore();
        services.AddXOutboxStore();
        services.AddSingleton<IPendingDomainEventsBuffer, PendingDomainEventsBuffer>();
        services.AddXAggregateStore<TestBankAccountAggregate>();
        services.AddXEventPublisher(EventRegistryMode.Dynamic);
        services.AddXEventHandlers(typeof(TestMoneyDepositedHandler).Assembly);

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    private sealed class SqliteDatabaseDirectory : IDisposable
    {
        private readonly string _rootDirectory;

        public SqliteDatabaseDirectory()
        {
            _rootDirectory = Path.Combine(Path.GetTempPath(), "xpandables-net-events", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_rootDirectory);
            EventStoreConnectionString = BuildConnectionString("event-store.db");
            OutboxConnectionString = BuildConnectionString("outbox-store.db");
        }

        public string EventStoreConnectionString { get; }
        public string OutboxConnectionString { get; }

        private string BuildConnectionString(string fileName)
        {
            string filePath = Path.Combine(_rootDirectory, fileName);
            return $"Data Source={filePath}";
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_rootDirectory))
                {
                    Directory.Delete(_rootDirectory, recursive: true);
                }
            }
            catch
            {
                // The files are best-effort artifacts for the test run.
            }
        }
    }
}
