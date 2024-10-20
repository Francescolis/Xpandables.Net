using System.Net;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Decorators;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Events;
using Xpandables.Net.Events.Defaults;
using Xpandables.Net.Responsibilities;
using Xpandables.Net.Responsibilities.Wrappers;

namespace Xpandables.Net.Test.UnitTests;

public sealed class TestQuery : IQueryAsync<string>, IUseValidation
{
}
public sealed class TestQueryAsyncHandler : IQueryAsyncHandler<TestQuery, string>
{
    public IAsyncEnumerable<string> HandleAsync(
        TestQuery query,
        CancellationToken cancellationToken = default) =>
        AsyncEnumerable.Empty<string>();
}

public sealed class EventPublisherSubscriberUnitTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventPublisherSubscriber _eventPublisherSubscriber;

    public EventPublisherSubscriberUnitTest()
    {
        var services = new ServiceCollection();
        services.AddScoped(typeof(IAsyncPipelineDecorator<,>), typeof(LoggingQueryAsyncHandlerDecorator<,>));
        services.AddScoped(typeof(IAsyncPipelineDecorator<,>), typeof(ValidationQueryAsyncHandlerDecorator<,>));
        services.AddScoped<IQueryAsyncHandler<TestQuery, string>, TestQueryAsyncHandler>();
        services.AddScoped<IDispatcher, Dispatcher>();
        services.AddTransient(typeof(QueryAsyncHandlerWrapper<,>));
        services.AddLogging();
        services.AddXEventPublisher();
        services.AddXValidatorDefault();
        _serviceProvider = services.BuildServiceProvider();
        _eventPublisherSubscriber = new EventPublisherSubscriber(_serviceProvider);
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnSuccess_WhenHandlersAreExecutedSuccessfully()
    {
        // Arrange
        TestEvent testEvent = new() { EventId = Guid.NewGuid(), EventVersion = 1 };
        _eventPublisherSubscriber.Subscribe<TestEvent>(e => { /* Handler logic */ });

        IDispatcher dispatcher = _serviceProvider.GetRequiredService<IDispatcher>();
        IQueryAsync<string> query = new TestQuery();
        var results = await dispatcher.SendAsync(query).ToListAsync();
        // Act
        var result = await _eventPublisherSubscriber.PublishAsync(testEvent);

        // Assert
        result.IsSuccessStatusCode.Should().BeTrue();
        result.Extensions.Should()
            .ContainEquivalentOf(
                new Collections.ElementEntry(
                    nameof(Event.EventId),
                    [testEvent.EventId.ToString()]));
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
        var result = await _eventPublisherSubscriber.PublishAsync(testEvent);

        // Assert
        result.IsSuccessStatusCode.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Extensions.Should()
            .ContainEquivalentOf(
                new Collections.ElementEntry(
                    nameof(Event.EventId),
                    [testEvent.EventId.ToString()]));
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
        result.IsSuccessStatusCode.Should().BeTrue();
        result.Result.Should().HaveCount(2);
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
