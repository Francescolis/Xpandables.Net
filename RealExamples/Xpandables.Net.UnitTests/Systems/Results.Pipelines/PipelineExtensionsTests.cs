using System.Linq;
using System.Results;
using System.Results.Pipelines;
using System.Results.Requests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Xpandables.Net.UnitTests.Systems.Results.Pipelines;

public sealed class PipelineExtensionsTests
{
    [Fact]
    public void AddXPipelineRequestHandler_ThrowsForInvalidType()
    {
        // Arrange
        var services = new ServiceCollection();

		// Act
		Func<IServiceCollection> action = () => services.AddXPipelineRequestHandler(typeof(object));

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void AddXPipelineRequestHandler_RegistersHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXPipelineRequestHandler(typeof(TestHandler));
        using ServiceProvider provider = services.BuildServiceProvider();
		IPipelineRequestHandler<TestRequest> handler = provider.GetRequiredService<IPipelineRequestHandler<TestRequest>>();

        // Assert
        Assert.IsType<TestHandler>(handler);
    }

    [Fact]
    public void AddXPipelineRequestHandler_RegistersScopedDescriptor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXPipelineRequestHandler(typeof(TestHandler));
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope1 = provider.CreateScope();
        using IServiceScope scope2 = provider.CreateScope();
		IPipelineRequestHandler<TestRequest> handler1 = scope1.ServiceProvider.GetRequiredService<IPipelineRequestHandler<TestRequest>>();
		IPipelineRequestHandler<TestRequest> handler2 = scope2.ServiceProvider.GetRequiredService<IPipelineRequestHandler<TestRequest>>();

        // Assert — scoped: different scopes yield different instances
        Assert.NotSame(handler1, handler2);
    }

    private sealed class TestRequest : IRequest;

    private sealed class TestHandler : IPipelineRequestHandler<TestRequest>
    {
        public Task<Result> HandleAsync(TestRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult<Result>(Result.Success());
    }
}
