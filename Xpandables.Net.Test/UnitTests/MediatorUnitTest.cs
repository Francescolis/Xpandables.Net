using System.Reflection;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Executions;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Test.UnitTests;
public class MediatorUnitTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;

    public MediatorUnitTest()
    {
        var services = new ServiceCollection();

        // Register mediator and handlers
        services.AddXMediator()
               .AddXHandlers(Assembly.GetExecutingAssembly())
               .AddXPipelineExceptionDecorator();

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    [Fact]
    public async Task SendAsync_WithValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new TestRequest { Data = "test data" };

        // Act
        var result = await _mediator.SendAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessStatusCode.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task SendAsync_WhenHandlerThrows_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new TestRequest { Data = "throw" };

        // Act
        var result = await _mediator.SendAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessStatusCode.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}

// Test classes
public sealed class TestRequest : IRequest
{
    public string Data { get; set; } = string.Empty;
}

public sealed class TestRequestHandler : IRequestHandler<TestRequest>
{
    public Task<ExecutionResult> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Data == "throw")
            throw new InvalidOperationException("Test exception");

        return Task.FromResult(ExecutionResult.Success());
    }
}