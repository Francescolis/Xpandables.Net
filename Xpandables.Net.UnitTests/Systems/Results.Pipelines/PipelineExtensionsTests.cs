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
        var action = () => services.AddXPipelineRequestHandler(typeof(object));

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
        using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IPipelineRequestHandler<TestRequest>>();

        // Assert
        Assert.IsType<TestHandler>(handler);
    }

    [Fact]
    public void AddXPipelineRequestHandler_RegistersTransientDescriptor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddXPipelineRequestHandler(typeof(TestHandler));

        // Assert
        ServiceDescriptor descriptor = Assert.Single(
            services.Where(descriptor => descriptor.ServiceType == typeof(IPipelineRequestHandler<>)
                && descriptor.ImplementationType == typeof(TestHandler)));

        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    private sealed class TestRequest : IRequest;

    private sealed class TestHandler : IPipelineRequestHandler<TestRequest>
    {
        public Task<Result> HandleAsync(TestRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult<Result>(Result.Success());
    }
}
