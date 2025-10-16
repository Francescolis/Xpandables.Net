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

public class AggregateTests
{
    [Fact]
    public void Aggregate_NewInstance_ShouldBeEmpty()
    {
        // Arrange & Act
        var aggregate = new BankAccountAggregate();

        // Assert
        aggregate.IsEmpty.Should().BeTrue();
        aggregate.StreamId.Should().Be(Guid.Empty);
        aggregate.StreamVersion.Should().Be(-1);
        aggregate.BusinessVersion.Should().Be(1);
        aggregate.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void Aggregate_Create_ShouldSetStreamIdAndVersion()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var accountNumber = "ACC-001";
        var owner = "John Doe";
        var initialBalance = 1000m;

        // Act
        var aggregate = BankAccountAggregate.Create(streamId, accountNumber, owner, initialBalance);

        // Assert
        aggregate.IsEmpty.Should().BeFalse();
        aggregate.StreamId.Should().Be(streamId);
        aggregate.StreamVersion.Should().Be(0);
        aggregate.AccountNumber.Should().Be(accountNumber);
        aggregate.Owner.Should().Be(owner);
        aggregate.Balance.Should().Be(initialBalance);
        aggregate.GetUncommittedEvents().Should().HaveCount(1);
    }

    [Fact]
    public void Aggregate_PushEvent_ShouldIncrementStreamVersion()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);
        aggregate.MarkEventsAsCommitted();

        // Act
        aggregate.Deposit(500m);

        // Assert
        aggregate.StreamVersion.Should().Be(1);
        aggregate.Balance.Should().Be(1500m);
        aggregate.GetUncommittedEvents().Should().HaveCount(1);
    }

    [Fact]
    public void Aggregate_MultipleEvents_ShouldMaintainCorrectState()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);

        // Act
        aggregate.Deposit(500m);
        aggregate.Deposit(200m);
        aggregate.Withdraw(300m);

        // Assert
        aggregate.StreamVersion.Should().Be(3);
        aggregate.Balance.Should().Be(1400m);
        aggregate.GetUncommittedEvents().Should().HaveCount(4); // Create + 3 operations
    }

    [Fact]
    public void Aggregate_DequeueUncommittedEvents_ShouldClearEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);
        aggregate.Deposit(500m);

        // Act
        var events = aggregate.DequeueUncommittedEvents();

        // Assert
        events.Should().HaveCount(2);
        aggregate.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void Aggregate_LoadFromHistory_ShouldRestoreState()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var originalAggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);
        originalAggregate.Deposit(500m);
        originalAggregate.Deposit(200m);
        var history = originalAggregate.GetUncommittedEvents();

        // Act
        var restoredAggregate = new BankAccountAggregate();
        restoredAggregate.Replay(history);

        // Assert
        restoredAggregate.StreamId.Should().Be(streamId);
        restoredAggregate.StreamVersion.Should().Be(2);
        restoredAggregate.Balance.Should().Be(1700m);
        restoredAggregate.AccountNumber.Should().Be("ACC-001");
        restoredAggregate.Owner.Should().Be("John Doe");
        restoredAggregate.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void Aggregate_LoadFromHistory_ShouldSortEventsByVersion()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var events = new List<IDomainEvent>
        {
            new MoneyDepositedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                StreamVersion = 2,
                Amount = 200m
            },
            new BankAccountCreatedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                StreamVersion = 0,
                AccountNumber = "ACC-001",
                Owner = "John Doe",
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
        var aggregate = new BankAccountAggregate();
        aggregate.Replay(events);

        // Assert
        aggregate.StreamVersion.Should().Be(2);
        aggregate.Balance.Should().Be(1700m);
    }

    [Fact]
    public void Aggregate_ExpectedStreamVersion_ShouldMatchStreamVersion()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);

        // Act & Assert
        aggregate.ExpectedStreamVersion.Should().Be(aggregate.StreamVersion);
        
        aggregate.Deposit(500m);
        aggregate.ExpectedStreamVersion.Should().Be(aggregate.StreamVersion);
    }

    [Fact]
    public void Aggregate_WithdrawMoreThanBalance_ShouldThrow()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);

        // Act
        var act = () => aggregate.Withdraw(1500m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient funds");
    }

    [Fact]
    public void Aggregate_DepositNegativeAmount_ShouldThrow()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);

        // Act
        var act = () => aggregate.Deposit(-100m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Deposit amount must be positive");
    }

    [Fact]
    public void Aggregate_WithdrawNegativeAmount_ShouldThrow()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);

        // Act
        var act = () => aggregate.Withdraw(-100m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Withdrawal amount must be positive");
    }

    [Fact]
    public void Aggregate_BusinessVersion_ShouldIncrementForSignificantEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);
        var initialBusinessVersion = aggregate.BusinessVersion;

        // Act
        aggregate.Deposit(500m);
        aggregate.MarkEventsAsCommitted();
        var history = new List<IDomainEvent>
        {
            new MoneyDepositedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                StreamVersion = 1,
                Amount = 500m
            }
        };
        var newAggregate = new BankAccountAggregate();
        newAggregate.Replay(history.Prepend(aggregate.DequeueUncommittedEvents().First()));

        // Assert - Business version increments during replay of significant events
        newAggregate.BusinessVersion.Should().BeGreaterThan(initialBusinessVersion);
    }

    [Fact]
    public void Aggregate_MarkEventsAsCommitted_ShouldClearUncommittedEvents()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);
        aggregate.Deposit(500m);

        // Act
        aggregate.MarkEventsAsCommitted();

        // Assert
        aggregate.GetUncommittedEvents().Should().BeEmpty();
    }

    [Fact]
    public void Aggregate_PushEvent_WithoutStreamId_ShouldThrow()
    {
        // Arrange
        var aggregate = new BankAccountAggregate();
        var @event = new MoneyDepositedEvent
        {
            StreamId = Guid.Empty,
            StreamName = "BankAccount",
            StreamVersion = 0,
            Amount = 100m
        };

        // Act
        var act = () => aggregate.PushVersioningEvent(@event);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Aggregate is not initialized. The first event must carry a non-empty StreamId.");
    }

    [Fact]
    public void Aggregate_PushEvent_PropagatesExistingStreamId()
    {
        // Arrange
        var streamId = Guid.NewGuid();
        var aggregate = BankAccountAggregate.Create(streamId, "ACC-001", "John Doe", 1000m);
        
        // Act
        aggregate.Deposit(500m);
        var events = aggregate.GetUncommittedEvents();

        // Assert
        events.Should().AllSatisfy(e => e.StreamId.Should().Be(streamId));
    }
}
