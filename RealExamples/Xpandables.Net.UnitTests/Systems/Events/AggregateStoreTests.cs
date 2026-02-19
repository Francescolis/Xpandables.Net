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
using System.Events.Domain;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Events;

public sealed class AggregateStoreTests
{
    [Fact]
    public async Task LoadAsync_WithExistingHistory_RehydratesAggregateState()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var history = new IDomainEvent[]
        {
            new AccountOpened
            {
                StreamId = accountId,
                OwnerName = "Ada",
                Amount = 150m,
                StreamVersion = 0
            },
            new MoneyDeposited
            {
                StreamId = accountId,
                Amount = 50m,
                StreamVersion = 1
            }
        };

        var eventStore = new FakeEventStore(history);
        var buffer = new PendingDomainEventsBuffer();
        var sut = new AggregateStore<TestBankAccountAggregate>(eventStore, buffer, new DefaultDomainEventEnricher(new AsyncLocalEventContextAccessor()));

        // Act
        TestBankAccountAggregate aggregate = await sut.LoadAsync(accountId);

        // Assert
        aggregate.StreamId.Should().Be(accountId);
        aggregate.Balance.Should().Be(200m);
        aggregate.StreamVersion.Should().Be(1);
        aggregate.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_WithPendingEvents_AppendsBatchAndQueuesDomainEvents()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var eventStore = new FakeEventStore();
        var buffer = new PendingDomainEventsBuffer();
        var sut = new AggregateStore<TestBankAccountAggregate>(eventStore, buffer, new DefaultDomainEventEnricher(new AsyncLocalEventContextAccessor()));
        var aggregate = TestBankAccountAggregate.Initialize();

        aggregate.Open(accountId, "Ada", 100m);
        aggregate.Deposit(50m);

        // Act
        await sut.SaveAsync(aggregate);

        // Assert
        eventStore.LastAppendRequest.Should().NotBeNull();
        eventStore.LastAppendRequest!.Value.StreamId.Should().Be(accountId);
        eventStore.LastAppendRequest!.Value.Events.Should().HaveCount(2);
        eventStore.LastAppendRequest!.Value.ExpectedVersion.Should().Be(-1);

		IReadOnlyCollection<PendingDomainEventsBatch> batches = buffer.Drain();
        batches.Should().ContainSingle();
        batches.Single().Events.Should().HaveCount(2);
    }
}
