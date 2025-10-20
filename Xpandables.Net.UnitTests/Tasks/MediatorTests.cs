/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.Net;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Cqrs;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Tasks;
using Xpandables.Net.Tasks.Pipelines;

namespace Xpandables.Net.UnitTests.Tasks;

public class MediatorTests
{
    private sealed class Ping : IRequest { }

    private sealed class PingHandler : IRequestHandler<Ping>
    {
        public Task<ExecutionResult> HandleAsync(Ping request, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResult.Ok().Build());
    }

    [Fact]
    public async Task SendAsync_ShouldResolvePipelineHandler_AndReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<Ping>, PingHandler>();
        services.AddTransient<IPipelineDecorator<Ping>, DummyDecorator>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new Ping());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed class DummyDecorator : IPipelineDecorator<Ping>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<Ping> context, RequestHandler nextHandler, CancellationToken cancellationToken)
            => nextHandler(cancellationToken);
    }

    // ========== Additional Mediator Completion Tests ==========

    [Fact]
    public async Task Mediator_NoHandler_ShouldReturnFailureResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        // No handler registered
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new Ping());

        // Assert - When no handler is registered, the mediator should return a failure result
        // rather than throwing an exception
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Mediator_MultipleHandlersRegistered_ShouldUseFirst()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<Ping>, PingHandler>();
        services.AddTransient<IRequestHandler<Ping>, PingHandler>(); // Duplicate
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new Ping());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    private sealed class SlowRequest : IRequest { }

    private sealed class SlowHandler : IRequestHandler<SlowRequest>
    {
        public async Task<ExecutionResult> HandleAsync(SlowRequest request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken);
            return ExecutionResult.Ok().Build();
        }
    }

    [Fact]
    public async Task Mediator_CancellationRequested_ShouldCancel()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<SlowRequest>, SlowHandler>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(10);

        // Act
        var act = async () => await mediator.SendAsync(new SlowRequest(), cts.Token);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    private sealed class DataRequest : IRequest<string>
    {
        public required string Value { get; init; }
    }

    private sealed class DataHandler : IRequestHandler<DataRequest, string>
    {
        public Task<ExecutionResult<string>> HandleAsync(DataRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExecutionResult.Ok($"Processed: {request.Value}").Build());
        }
    }

    [Fact]
    public async Task Mediator_GenericRequest_ShouldReturnTypedResult()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<DataRequest>, DataHandler>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        ExecutionResult<string> result = await mediator.SendAsync(new DataRequest { Value = "test" });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Processed: test");
    }

    private sealed class ValidatedRequest : IRequest { }

    private sealed class ValidationDecorator : IPipelineDecorator<ValidatedRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            RequestContext<ValidatedRequest> context,
            RequestHandler nextHandler,
            CancellationToken cancellationToken)
        {
            context["validated"] = true;
            return nextHandler(cancellationToken);
        }
    }

    private sealed class ValidatedHandler : IRequestContextHandler<ValidatedRequest>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<ValidatedRequest> context, CancellationToken cancellationToken = default)
        {
            context.TryGetItem("validated", out object? value).Should().BeTrue();
            value.Should().Be(true);
            return Task.FromResult(ExecutionResult.Ok().Build());
        }
    }

    [Fact]
    public async Task Mediator_DecoratorSetsContext_HandlerReceivesIt()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IPipelineDecorator<ValidatedRequest>, ValidationDecorator>();
        services.AddTransient<IRequestHandler<ValidatedRequest>, ValidatedHandler>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new ValidatedRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
