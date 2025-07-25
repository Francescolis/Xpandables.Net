using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Test.UnitTests;

public sealed record TestQuery : IRequest<string>
{
    public required string Query { get; set; }
}

public sealed class TestQueryHander : IRequestHandler<TestQuery>
{
    public async Task<ExecutionResult> HandleAsync(
        TestQuery query, CancellationToken cancellationToken)
    {
        await Task.Yield();
        return ExecutionResult.Ok(query.Query).Build();
    }
}

public sealed class EventPublisherSubscriberUnitTest
{
    private readonly PublisherSubscriber _publisherSubscriber;
    private readonly IServiceProvider _serviceProvider;

    public EventPublisherSubscriberUnitTest()
    {
        ServiceCollection services = new();
        services.AddXHandlers();
        _serviceProvider = services.BuildServiceProvider();
        _publisherSubscriber = new PublisherSubscriber(_serviceProvider);
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnSuccess_WhenHandlersAreExecutedSuccessfully()
    {
        // Arrange
        TestIntegrationEvent testIntegrationEvent = new() { EventId = Guid.CreateVersion7(), EventVersion = 1 };
        _publisherSubscriber.Subscribe<TestIntegrationEvent>(e =>
        {
            /* Handler logic */
        });

        // Act
        await _publisherSubscriber.PublishAsync(testIntegrationEvent);

        // Assert
        // No exception should be thrown
    }

    [Fact]
    public async Task PublishAsync_ShouldReturnFailure_WhenHandlerThrowsException()
    {
        // Arrange
        TestIntegrationEvent testIntegrationEvent = new() { EventId = Guid.CreateVersion7(), EventVersion = 1 };
        _publisherSubscriber
            .Subscribe<TestIntegrationEvent>(e =>
                throw new InvalidOperationException("Test exception"));

        // Act
        Func<Task> result = () => _publisherSubscriber.PublishAsync(testIntegrationEvent);

        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>();
    }

    private record TestIntegrationEvent : IntegrationEvent
    {
    }
}