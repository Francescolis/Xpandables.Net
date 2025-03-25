using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Test.UnitTests;

public sealed record TestQuery : IRequest<string> { public required string Query { get; set; } }
public sealed class TestQueryHander : IRequestHandler<TestQuery, string>
{
    public Task<IExecutionResult<string>> HandleAsync(
        TestQuery query, CancellationToken cancellationToken) =>
        Task.FromResult(ExecutionResults
            .Ok(query.Query)
            .Build());
}

public sealed class EventPublisherSubscriberUnitTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PublisherSubscriber _publisherSubscriber;

    public EventPublisherSubscriberUnitTest()
    {
        var services = new ServiceCollection();
        services.AddXHandlers();
        _serviceProvider = services.BuildServiceProvider();
        _publisherSubscriber = new PublisherSubscriber(_serviceProvider);
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnSuccess_WhenHandlersAreExecutedSuccessfully()
    {
        // Arrange
        TestEvent testEvent = new() { EventId = Guid.CreateVersion7(), EventVersion = 1 };
        _publisherSubscriber.Subscribe<TestEvent>(e => { /* Handler logic */ });

        // Act
        await _publisherSubscriber.PublishAsync(testEvent);

        // Assert
        // No exception should be thrown
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnFailure_WhenHandlerThrowsException()
    {
        // Arrange
        TestEvent testEvent = new() { EventId = Guid.CreateVersion7(), EventVersion = 1 };
        _publisherSubscriber
            .Subscribe<TestEvent>(e =>
                throw new InvalidOperationException("Test exception"));

        // Act
        var result = () => _publisherSubscriber.PublishAsync(testEvent);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>();
    }

    private record TestEvent : EventIntegration
    {
    }

}
