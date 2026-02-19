using System.Net;
using System.Results;
using System.Results.Pipelines;
using System.Results.Requests;
using System.Results.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Xpandables.Net.UnitTests.Systems.Results.Tasks;

public sealed class MediatorTests
{
    [Fact]
    public async Task SendAsync_ResolvesHandlerAndReturnsResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IPipelineRequestHandler<TestRequest>, SuccessHandler>();
        var mediator = new Mediator(services.BuildServiceProvider());
        var request = new TestRequest();

		// Act
		Result result = await mediator.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task SendAsync_WithoutExceptionDecorator_PropagatesException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IPipelineRequestHandler<TestRequest>, ThrowingHandler>();
        var mediator = new Mediator(services.BuildServiceProvider());
        var request = new TestRequest();

        // Act & Assert — Mediator is a pure dispatcher; without PipelineExceptionDecorator,
        // unhandled exceptions propagate to the caller.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(request));
    }

    [Fact]
    public async Task SendAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var mediator = new Mediator(new ServiceCollection().BuildServiceProvider());

		// Act
		Func<Task<Result>> action = async () => await mediator.SendAsync<TestRequest>(null!);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(action);
    }

    [Fact]
    public async Task SendAsync_WhenCancelled_PropagatesOperationCanceledException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IPipelineRequestHandler<TestRequest>, CancellationAwareHandler>();
        var mediator = new Mediator(services.BuildServiceProvider());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

		// Act
		Func<Task<Result>> action = async () => await mediator.SendAsync(new TestRequest(), cancellation.Token);

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(action);
    }

    [Fact]
    public async Task SendAsync_WhenHandlerThrowsResultException_Rethrows()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IPipelineRequestHandler<TestRequest>, ResultExceptionHandler>();
        var mediator = new Mediator(services.BuildServiceProvider());

		// Act
		Func<Task<Result>> action = async () => await mediator.SendAsync(new TestRequest());

        // Assert
        await Assert.ThrowsAsync<ResultException>(action);
    }

    private sealed class TestRequest : IRequest;

    private sealed class SuccessHandler : IPipelineRequestHandler<TestRequest>
    {
        public Task<Result> HandleAsync(TestRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult<Result>(new SuccessResultBuilder(HttpStatusCode.OK));
    }

    private sealed class ThrowingHandler : IPipelineRequestHandler<TestRequest>
    {
        public Task<Result> HandleAsync(TestRequest request, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("boom");
    }

    private sealed class CancellationAwareHandler : IPipelineRequestHandler<TestRequest>
    {
        public Task<Result> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Result>(Result.Success());
        }
    }

    private sealed class ResultExceptionHandler : IPipelineRequestHandler<TestRequest>
    {
        public Task<Result> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            throw new ResultException(Result.Failure().WithError("key", "error").Build());
        }
    }
}
