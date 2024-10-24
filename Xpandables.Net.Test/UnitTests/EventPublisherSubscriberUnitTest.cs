using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events;
using Xpandables.Net.Events.Defaults;
using Xpandables.Net.Operations;
using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Test.UnitTests;

public sealed record TestQuery : IQuery<string> { public required string Query { get; set; } }
public sealed class TestQueryHander : IQueryHandler<TestQuery, string>
{
    public Task<IOperationResult<string>> HandleAsync(
        TestQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(OperationResults
            .Ok(query.Query)
            .Build());
}

public sealed class EventPublisherSubscriberUnitTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventPublisherSubscriber _eventPublisherSubscriber;

    public EventPublisherSubscriberUnitTest()
    {
        var services = new ServiceCollection();
        services.AddXDispatcherHandlers();
        _serviceProvider = services.BuildServiceProvider();
        _eventPublisherSubscriber = new EventPublisherSubscriber(_serviceProvider);
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnSuccess_WhenHandlersAreExecutedSuccessfully()
    {
        // Arrange
        TestEvent testEvent = new() { EventId = Guid.NewGuid(), EventVersion = 1 };
        _eventPublisherSubscriber.Subscribe<TestEvent>(e => { /* Handler logic */ });

        // Act
        await _eventPublisherSubscriber.PublishAsync(testEvent);

        // Assert
        // No exception should be thrown
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnFailure_WhenHandlerThrowsException()
    {
        // Arrange
        TestEvent testEvent = new() { EventId = Guid.NewGuid(), EventVersion = 1 };
        _eventPublisherSubscriber
            .Subscribe<TestEvent>(e =>
                throw new InvalidOperationException("Test exception"));

        // Act
        var result = () => _eventPublisherSubscriber.PublishAsync(testEvent);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PublishAsync_MultipleEvents_ShouldReturnSuccess_WhenHandlersAreExecutedSuccessfully()
    {
        // Arrange
        var testEvents = new List<TestEvent>
            {
                new() { EventId = Guid.NewGuid(),EventVersion=1 },
                new() { EventId = Guid.NewGuid(), EventVersion = 1 }
            };
        _eventPublisherSubscriber.Subscribe<TestEvent>(e => { /* Handler logic */ });

        // Act
        var result = await _eventPublisherSubscriber.PublishAsync(testEvents);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void Subscribe_ShouldAddActionHandler()
    {
        // Arrange
        Action<TestEvent> handler = e => { /* Handler logic */ };

        // Act
        _eventPublisherSubscriber.Subscribe(handler);

        // Assert
        var handlers = _eventPublisherSubscriber.GetHandlersOf<TestEvent>();
        handlers.Should().Contain(handler);
    }

    [Fact]
    public void Subscribe_ShouldAddFuncHandler()
    {
        // Arrange
        Func<TestEvent, Task> handler = e => Task.CompletedTask;

        // Act
        _eventPublisherSubscriber.Subscribe(handler);

        // Assert
        var handlers = _eventPublisherSubscriber.GetHandlersOf<TestEvent>();
        handlers.Should().Contain(handler);
    }

    [Fact]
    public void Dispose_ShouldClearHandlers()
    {
        // Arrange
        _eventPublisherSubscriber.Subscribe<TestEvent>(e => { /* Handler logic */ });

        // Act
        _eventPublisherSubscriber.Dispose();

        // Assert
        var handlers = _eventPublisherSubscriber.GetHandlersOf<TestEvent>();
        handlers.Should().BeEmpty();
    }

    private record TestEvent : EventIntegration
    {
    }

}
