/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
namespace Xpandables.Net.UnitTests;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xpandables.Net;
using Xpandables.Net.Aggregates;
using Xpandables.Net.Events;
using Xpandables.Net.Internals;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.Collections;

public static class TestServiceCollection
{
    public static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register necessary services and handlers
        services.AddTransient<IEventPublisher, MockEventPublisher>();
        services.AddTransient(typeof(IRequestHandler<,>),
            typeof(MockRequestHandler<,>));
        services.AddTransient(typeof(IRequestHandler<>),
            typeof(MockRequestHandler<>));
        services.AddTransient(typeof(IRequestAggregateHandler<,>),
            typeof(MockRequestAggregateHandler<,>));
        services.AddTransient(typeof(IAsyncRequestHandler<,>),
            typeof(MockAsyncRequestHandler<,>));
        services.AddTransient<IDispatcher, Dispatcher>();

        return services.BuildServiceProvider();
    }
}

public class MockEventPublisher : IEventPublisher
{
    public Task<IOperationResult> PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : notnull, IEvent
        => Task.FromResult(OperationResults.Success().Build());
}

public class MockRequestHandler<TRequest, TResponse> :
    IRequestHandler<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    public Task<IOperationResult<TResponse>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResults.Success<TResponse>().Build());
}

public class MockRequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : notnull, IRequest
{
    public Task<IOperationResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResults.Success().Build());
}

public class MockRequestAggregateHandler<TRequest, TAggregate> :
    IRequestAggregateHandler<TRequest, TAggregate>
    where TRequest : class, IRequestAggregate<TAggregate>
    where TAggregate : class, IAggregate
{
    public Task<IOperationResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResults.Success().Build());
}

public class MockAsyncRequestHandler<TRequest, TResponse> :
    IAsyncRequestHandler<TRequest, TResponse>
    where TRequest : notnull, IAsyncRequest<TResponse>
{
    public IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
        => AsyncEnumerable.Empty<TResponse>();
}

public class DistributorTests
{
    private readonly IDispatcher _distributor;

    public DistributorTests()
    {
        var serviceProvider = TestServiceCollection.ConfigureServices();
        _distributor = serviceProvider.GetRequiredService<IDispatcher>();
    }

    [Fact]
    public async Task SendAsync_Request_ShouldReturnOperationResult()
    {
        // Arrange
        var request = Mock.Of<IRequest>();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _distributor.SendAsync(request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendAsync_RequestAggregate_ShouldReturnOperationResult()
    {
        // Arrange
        var request = Mock.Of<IRequestAggregate<IAggregate>>();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _distributor
            .SendAsync<IRequestAggregate<IAggregate>, IAggregate>(
            request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetAsync_Request_ShouldReturnOperationResultWithResponse()
    {
        // Arrange
        var request = Mock.Of<IRequest<object>>();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _distributor
            .GetAsync<IRequest<object>, object>(request, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task FetchAsync_Request_ShouldReturnAsyncEnumerable()
    {
        // Arrange
        var request = Mock.Of<IAsyncRequest<object>>();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = _distributor
            .FetchAsync<IAsyncRequest<object>, object>(
            request, cancellationToken);

        // Assert
        await foreach (var item in result)
        {
            Assert.NotNull(item);
        }
    }

    [Fact]
    public async Task PublishAsync_Event_ShouldReturnOperationResult()
    {
        // Arrange
        var @event = Mock.Of<IEvent>();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _distributor.PublishAsync(@event, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
    }
}
