using FluentAssertions;

using Xpandables.Net.Events;

namespace Xpandables.Net.Test.UnitTests;
public class AggregateUnitTest
{
    [Fact]
    public void PushEvent_ShouldIncreaseVersion_AndStoreEvent()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var testEvent = new TestCreated { AggregateId = Guid.NewGuid(), Name = "Test Name" };

        // Act
        aggregate.PushEvent(testEvent);

        // Assert
        aggregate.Version.Should().Be(1);
        aggregate.GetUncommittedEvents().Should().ContainSingle();
        aggregate.GetUncommittedEvents().First().Should().BeOfType<TestCreated>();
        aggregate.Name.Should().Be("Test Name");
    }

    [Fact]
    public void PushMultipleEvents_ShouldIncreaseVersionSequentially()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var id = Guid.NewGuid();
        List<IDomainEvent> events =
        [
                new TestCreated { AggregateId = id, Name = "Name 1" },
                new TestNameUpdated { AggregateId = id, Name = "Name 2" },
                new TestNameUpdated { AggregateId = id, Name = "Name 3" }
            ];

        // Act
        foreach (var @event in events)
        {
            aggregate.PushEvent(@event);
        }

        // Assert
        aggregate.Version.Should().Be(3);
        aggregate.GetUncommittedEvents().Should().HaveCount(3);
        aggregate.Name.Should().Be("Name 3"); // Last event's name
    }

    [Fact]
    public void LoadFromHistory_ShouldReconstructAggregateState()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var id = Guid.NewGuid();
        var historicalEvents = new[]
        {
                new TestCreated { AggregateId = id, Name = "Historical Name 1" }.WithVersion(1),
                new TestNameUpdated { AggregateId = id, Name = "Historical Name 2" }.WithVersion(2)
            };

        // Act
        aggregate.LoadFromHistory(historicalEvents);

        // Assert
        aggregate.Version.Should().Be(2);
        aggregate.KeyId.Should().Be(id);
        aggregate.Name.Should().Be("Historical Name 2");
        aggregate.GetUncommittedEvents().Should().BeEmpty(); // Historical events are not uncommitted
    }

    [Fact]
    public void MarkEventsAsCommitted_ShouldClearUncommittedEvents()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var testEvent = new TestCreated { AggregateId = Guid.NewGuid(), Name = "Test Name" };
        aggregate.PushEvent(testEvent);

        // Act
        aggregate.MarkEventsAsCommitted();

        // Assert
        aggregate.GetUncommittedEvents().Should().BeEmpty();
        aggregate.Version.Should().Be(1); // Version should remain unchanged
        aggregate.Name.Should().Be("Test Name"); // State should remain unchanged
    }

    [Fact]
    public void PushDuplicateEvent_ShouldNotApplyEventTwice()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var id = Guid.NewGuid();
        var duplicateEvent = new TestCreated { AggregateId = id, Name = "Test Name" };

        // Act
        aggregate.PushEvent(duplicateEvent);
        aggregate.PushEvent(duplicateEvent); // Same event pushed twice

        // Assert
        aggregate.Version.Should().Be(1); // Version should only increment once
        aggregate.GetUncommittedEvents().Should().ContainSingle();
    }

    [Fact]
    public void PushEvent_WithUnregisteredEventType_ShouldThrowException()
    {
        // Arrange
        var aggregate = new TestAggregate();
        var unregisteredEvent = new UnregisteredEvent { AggregateId = Guid.NewGuid() };

        // Act & Assert
        var act = () => aggregate.PushEvent(unregisteredEvent);
        act.Should().Throw<UnauthorizedAccessException>()
           .WithMessage($"The submitted event {nameof(UnregisteredEvent)} is not authorized.");
    }
}

// Test classes
public class TestAggregate : Aggregate
{
    public string Name { get; private set; } = string.Empty;

    public TestAggregate()
    {
        On<TestCreated>(HandleCreated);
        On<TestNameUpdated>(HandleNameUpdated);
    }

    private void HandleCreated(TestCreated @event) => Name = @event.Name;

    private void HandleNameUpdated(TestNameUpdated @event) => Name = @event.Name;
}

public sealed record TestCreated : DomainEvent<TestAggregate>
{
    public required string Name { get; init; }
}

public sealed record TestNameUpdated : DomainEvent<TestAggregate>
{
    public required string Name { get; init; }
}

public sealed record UnregisteredEvent : DomainEvent<TestAggregate>
{
    // Empty event for testing unregistered event handling
}