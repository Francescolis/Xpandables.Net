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
using FluentAssertions;

using Xpandables.Net.Events;

namespace Xpandables.Net.UnitTests.Events;

/// <summary>
/// Unit tests for IEventSourcing interface implementation in Aggregate base class.
/// Tests event sourcing capabilities including event tracking, replay, and versioning.
/// </summary>
public class EventSourcingTests
{
    [Fact]
    public void EventSourcing_GetUncommittedEvents_ShouldReturnPendingEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-ES-001", "Event Sourcing Test", 1000m);

        // Act
        var uncommittedEvents = aggregate.GetUncommittedEvents();

        // Assert
        uncommittedEvents.Should().HaveCount(1);
        uncommittedEvents.First().Should().BeOfType<BankAccountCreatedEvent>();
    }

    [Fact]
    public void EventSourcing_DequeueUncommittedEvents_ShouldReturnAndClearEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-ES-002", "Dequeue Test", 1000m);
        aggregate.Deposit(500m);

        // Act
        var events = aggregate.DequeueUncommittedEvents();

        // Assert
        events.Should().HaveCount(2);
        aggregate.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void EventSourcing_MarkEventsAsCommitted_ShouldClearUncommittedEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-ES-003", "Mark Committed Test", 1000m);

        // Act
        aggregate.MarkEventsAsCommitted();

        // Assert
        aggregate.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void EventSourcing_Replay_ShouldRestoreAggregateState()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var originalAggregate = BankAccountAggregate.Create(streamId, "ACC-ES-004", "Replay Test", 1000m);
        originalAggregate.Deposit(500m);
        originalAggregate.Withdraw(200m);
        
        var history = originalAggregate.GetUncommittedEvents();

        // Act
        var replayedAggregate = new BankAccountAggregate();
        replayedAggregate.Replay(history);

        // Assert
        replayedAggregate.StreamId.Should().Be(streamId);
        replayedAggregate.Balance.Should().Be(1300m);
        replayedAggregate.AccountNumber.Should().Be("ACC-ES-004");
        replayedAggregate.StreamVersion.Should().Be(2);
    }

    [Fact]
    public void EventSourcing_LoadFromHistorySingleEvent_ShouldApplyEvent()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = new BankAccountAggregate();
        var @event = new BankAccountCreatedEvent
        {
            StreamId = streamId,
            StreamName = "BankAccount",
            StreamVersion = 0,
            AccountNumber = "ACC-ES-005",
            Owner = "Load Single Test",
            InitialBalance = 2000m
        };

        // Act
        aggregate.LoadFromHistory(@event);

        // Assert
        aggregate.StreamId.Should().Be(streamId);
        aggregate.Balance.Should().Be(2000m);
        aggregate.StreamVersion.Should().Be(0);
    }

    [Fact]
    public void EventSourcing_LoadFromHistoryMultipleEvents_ShouldApplyInOrder()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = new BankAccountAggregate();
        var events = new List<IDomainEvent>
        {
            new BankAccountCreatedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                StreamVersion = 0,
                AccountNumber = "ACC-ES-006",
                Owner = "Load Multiple Test",
                InitialBalance = 1000m
            },
            new MoneyDepositedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                StreamVersion = 1,
                Amount = 500m
            }
        };

        // Act
        aggregate.LoadFromHistory(events);

        // Assert
        aggregate.Balance.Should().Be(1500m);
        aggregate.StreamVersion.Should().Be(1);
    }

    [Fact]
    public void EventSourcing_PushEvent_ShouldAddToUncommittedEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-ES-007", "Push Test", 1000m);
        aggregate.MarkEventsAsCommitted();

        // Act
        aggregate.Deposit(300m);

        // Assert
        aggregate.GetUncommittedEvents().Should().HaveCount(1);
        aggregate.GetUncommittedEvents().First().Should().BeOfType<MoneyDepositedEvent>();
    }

    [Fact]
    public void EventSourcing_PushVersioningEvent_ShouldIncrementVersion()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-ES-008", "Versioning Test", 1000m);
        var initialVersion = aggregate.StreamVersion;

        // Act
        aggregate.Deposit(100m);

        // Assert
        aggregate.StreamVersion.Should().Be(initialVersion + 1);
    }

    [Fact]
    public void EventSourcing_PushVersioningEventWithFactory_ShouldUseNextVersion()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-ES-009", "Factory Test", 1000m);

        // Act
        aggregate.PushVersioningEvent<MoneyDepositedEvent>(version => new MoneyDepositedEvent
        {
            StreamId = streamId,
            StreamName = "BankAccount",
            StreamVersion = version,
            Amount = 250m
        });

        // Assert
        aggregate.Balance.Should().Be(1250m);
        aggregate.StreamVersion.Should().Be(1);
    }

    [Fact]
    public void EventSourcing_ReplayOutOfOrderEvents_ShouldSortByVersion()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = new BankAccountAggregate();
        
        // Create events out of order
        var events = new List<IDomainEvent>
        {
            new MoneyDepositedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                StreamVersion = 2,
                Amount = 300m
            },
            new BankAccountCreatedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                StreamVersion = 0,
                AccountNumber = "ACC-ES-010",
                Owner = "Sort Test",
                InitialBalance = 1000m
            },
            new MoneyDepositedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                StreamVersion = 1,
                Amount = 200m
            }
        };

        // Act
        aggregate.LoadFromHistory(events);

        // Assert
        aggregate.Balance.Should().Be(1500m); // 1000 + 200 + 300
        aggregate.StreamVersion.Should().Be(2);
    }

    [Fact]
    public void EventSourcing_MultipleReplays_ShouldProduceConsistentState()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var originalAggregate = BankAccountAggregate.Create(streamId, "ACC-ES-011", "Multiple Replay Test", 5000m);
        originalAggregate.Deposit(1000m);
        originalAggregate.Withdraw(500m);
        originalAggregate.Deposit(250m);
        
        var history = originalAggregate.GetUncommittedEvents();

        // Act
        var replay1 = new BankAccountAggregate();
        replay1.Replay(history);
        
        var replay2 = new BankAccountAggregate();
        replay2.Replay(history);

        // Assert
        replay1.Balance.Should().Be(replay2.Balance);
        replay1.StreamVersion.Should().Be(replay2.StreamVersion);
        replay1.AccountNumber.Should().Be(replay2.AccountNumber);
    }

    [Fact]
    public void EventSourcing_EmptyHistory_ShouldNotChangeAggregate()
    {
        // Arrange
        var aggregate = new BankAccountAggregate();
        var emptyHistory = new List<IDomainEvent>();

        // Act
        aggregate.LoadFromHistory(emptyHistory);

        // Assert
        aggregate.IsEmpty.Should().BeTrue();
        aggregate.StreamVersion.Should().Be(-1);
    }
}
