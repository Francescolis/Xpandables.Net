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
/// Unit tests for pending domain events buffer functionality.
/// Tests event collection, batching, and commit callbacks.
/// </summary>
public class PendingDomainEventsBufferTests
{
    [Fact]
    public void PendingEventsBuffer_AddRange_ShouldStoreEvents()
    {
        // Arrange
        var buffer = new PendingDomainEventsBuffer();
        var streamId = Guid.NewGuid();
        var events = new List<IDomainEvent>
        {
            new BankAccountCreatedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                AccountNumber = "BUF-001",
                Owner = "Buffer Test",
                InitialBalance = 1000m
            }
        };
#pragma warning disable CS0219 // Variable is assigned but its value is never used
        var commitCalled = false;
#pragma warning restore CS0219 // Variable is assigned but its value is never used
        Action onCommit = () => commitCalled = true;

        // Act
        buffer.AddRange(events, onCommit);
        var batches = buffer.Drain();

        // Assert
        batches.Should().HaveCount(1);
        batches.First().Events.Should().HaveCount(1);
    }

    [Fact]
    public void PendingEventsBuffer_Drain_ShouldClearBuffer()
    {
        // Arrange
        var buffer = new PendingDomainEventsBuffer();
        var streamId = Guid.NewGuid();
        var events = new List<IDomainEvent>
        {
            new MoneyDepositedEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                Amount = 500m
            }
        };

        buffer.AddRange(events, () => { });

        // Act
        var firstDrain = buffer.Drain();
        var secondDrain = buffer.Drain();

        // Assert
        firstDrain.Should().HaveCount(1);
        secondDrain.Should().BeEmpty();
    }

    [Fact]
    public void PendingEventsBuffer_OnCommitted_ShouldInvokeCallback()
    {
        // Arrange
        var buffer = new PendingDomainEventsBuffer();
        var streamId = Guid.NewGuid();
        var events = new List<IDomainEvent>
        {
            new MoneyWithdrawnEvent
            {
                StreamId = streamId,
                StreamName = "BankAccount",
                Amount = 200m
            }
        };
        var commitCalled = false;
        Action onCommit = () => commitCalled = true;

        buffer.AddRange(events, onCommit);
        var batches = buffer.Drain();

        // Act
        foreach (var batch in batches)
        {
            batch.OnCommitted();
        }

        // Assert
        commitCalled.Should().BeTrue();
    }

    [Fact]
    public void PendingEventsBuffer_MultipleBatches_ShouldMaintainOrder()
    {
        // Arrange
        var buffer = new PendingDomainEventsBuffer();
        var streamId1 = Guid.NewGuid();
        var streamId2 = Guid.NewGuid();

        var batch1 = new List<IDomainEvent>
        {
            new BankAccountCreatedEvent
            {
                StreamId = streamId1,
                StreamName = "BankAccount",
                AccountNumber = "BUF-002",
                Owner = "First",
                InitialBalance = 1000m
            }
        };

        var batch2 = new List<IDomainEvent>
        {
            new BankAccountCreatedEvent
            {
                StreamId = streamId2,
                StreamName = "BankAccount",
                AccountNumber = "BUF-003",
                Owner = "Second",
                InitialBalance = 2000m
            }
        };

        // Act
        buffer.AddRange(batch1, () => { });
        buffer.AddRange(batch2, () => { });
        var batches = buffer.Drain().ToList();

        // Assert
        batches.Should().HaveCount(2);
        ((BankAccountCreatedEvent)batches[0].Events.First()).AccountNumber.Should().Be("BUF-002");
        ((BankAccountCreatedEvent)batches[1].Events.First()).AccountNumber.Should().Be("BUF-003");
    }

    [Fact]
    public void PendingEventsBuffer_EmptyDrain_ShouldReturnEmptyCollection()
    {
        // Arrange
        var buffer = new PendingDomainEventsBuffer();

        // Act
        var batches = buffer.Drain();

        // Assert
        batches.Should().BeEmpty();
    }
}
