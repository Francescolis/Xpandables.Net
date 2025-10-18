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

using Xpandables.Net.Async;
using Xpandables.Net.Collections;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.UnitTests.Tasks;

/// <summary>
/// Comprehensive tests for Mediator covering advanced scenarios,
/// error handling, and edge cases.
/// </summary>
public class MediatorAdvancedTests
{
    // ========== Multiple Decorators Chain ==========
    private sealed class ChainedRequest : IRequest { }

    private sealed class ChainedHandler : IRequestHandler<ChainedRequest>
    {
        public Task<ExecutionResult> HandleAsync(ChainedRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResultExtensions.Ok().Build());
    }

    private sealed class FirstDecorator : IPipelineDecorator<ChainedRequest>
    {
        public async Task<ExecutionResult> HandleAsync(
            RequestContext<ChainedRequest> context,
            RequestHandler nextHandler,
            CancellationToken cancellationToken)
        {
            context["first"] = "executed";
            return await nextHandler(cancellationToken);
        }
    }

    private sealed class SecondDecorator : IPipelineDecorator<ChainedRequest>
    {
        public async Task<ExecutionResult> HandleAsync(
            RequestContext<ChainedRequest> context,
            RequestHandler nextHandler,
            CancellationToken cancellationToken)
        {
            context["second"] = "executed";
            context.TryGetItem("first", out object? first).Should().BeTrue();
            first.Should().Be("executed");
            return await nextHandler(cancellationToken);
        }
    }

    private sealed class ThirdDecorator : IPipelineDecorator<ChainedRequest>
    {
        public async Task<ExecutionResult> HandleAsync(
            RequestContext<ChainedRequest> context,
            RequestHandler nextHandler,
            CancellationToken cancellationToken)
        {
            context["third"] = "executed";
            context.TryGetItem("second", out object? second).Should().BeTrue();
            second.Should().Be("executed");
            return await nextHandler(cancellationToken);
        }
    }

    [Fact]
    public async Task Mediator_MultipleDecoratorsChain_ShouldExecuteInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<ChainedRequest>, ChainedHandler>();
        services.AddTransient<IPipelineDecorator<ChainedRequest>, FirstDecorator>();
        services.AddTransient<IPipelineDecorator<ChainedRequest>, SecondDecorator>();
        services.AddTransient<IPipelineDecorator<ChainedRequest>, ThirdDecorator>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new ChainedRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ========== Cancellation Token Tests ==========
    private sealed class LongRunningRequest : IRequest { }

    private sealed class LongRunningHandler : IRequestHandler<LongRunningRequest>
    {
        public async Task<ExecutionResult> HandleAsync(
            LongRunningRequest request,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(5000, cancellationToken);
            return ExecutionResultExtensions.Ok().Build();
        }
    }

    [Fact]
    public async Task Mediator_WithCancellationToken_ShouldCancelOperation()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<LongRunningRequest>, LongRunningHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act
        var act = async () => await mediator.SendAsync(new LongRunningRequest(), cts.Token);

        // Assert
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    // ========== Complex Context Sharing ==========
    private sealed class ContextSharingRequest : IRequest { }

    private sealed class ContextPreHandler1 : IRequestPreHandler<ContextSharingRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            RequestContext<ContextSharingRequest> context,
            CancellationToken cancellationToken = default)
        {
            context["data"] = new List<string> { "pre1" };
            return Task.FromResult(ExecutionResultExtensions.Ok().Build());
        }
    }

    private sealed class ContextPreHandler2 : IRequestPreHandler<ContextSharingRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            RequestContext<ContextSharingRequest> context,
            CancellationToken cancellationToken = default)
        {
            if (context.TryGetItem("data", out object? value) && value is List<string> list)
            {
                list.Add("pre2");
            }
            return Task.FromResult(ExecutionResultExtensions.Ok().Build());
        }
    }

    private sealed class ContextMainHandler : IRequestContextHandler<ContextSharingRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            RequestContext<ContextSharingRequest> context,
            CancellationToken cancellationToken = default)
        {
            if (context.TryGetItem("data", out object? value) && value is List<string> list)
            {
                list.Add("main");
            }
            return Task.FromResult(ExecutionResultExtensions.Ok().Build());
        }
    }

    private sealed class ContextPostHandler : IRequestPostHandler<ContextSharingRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            RequestContext<ContextSharingRequest> context,
            ExecutionResult response,
            CancellationToken cancellationToken = default)
        {
            if (context.TryGetItem("data", out object? value) && value is List<string> list)
            {
                list.Should().Contain("pre1");
                list.Should().Contain("pre2");
                list.Should().Contain("main");
            }
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task Mediator_ComplexContextSharing_ShouldWorkAcrossHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelinePreDecorator();
        services.AddXPipelinePostDecorator();
        services.AddTransient<IRequestPreHandler<ContextSharingRequest>, ContextPreHandler1>();
        services.AddTransient<IRequestPreHandler<ContextSharingRequest>, ContextPreHandler2>();
        services.AddTransient<IRequestHandler<ContextSharingRequest>, ContextMainHandler>();
        services.AddTransient<IRequestPostHandler<ContextSharingRequest>, ContextPostHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new ContextSharingRequest());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ========== Streaming with Pagination ==========
    private sealed class PagedStreamRequest : IStreamRequest<int>
    {
        public required int PageSize { get; init; }
        public required int TotalItems { get; init; }
    }

    private sealed class PagedStreamHandler : IStreamRequestHandler<PagedStreamRequest, int>
    {
        public Task<ExecutionResult<IAsyncPagedEnumerable<int>>> HandleAsync(
            PagedStreamRequest request,
            CancellationToken cancellationToken = default)
        {
            async IAsyncEnumerable<int> Source()
            {
                for (int i = 1; i <= request.TotalItems; i++)
                {
                    yield return i;
                    await Task.Delay(10, cancellationToken);
                }
            }

            var paged = new AsyncPagedEnumerable<int>(
                Source(),
                ct => new ValueTask<Pagination>(Pagination.Create(
                    request.PageSize,
                    1,
                    null,
                    request.TotalItems)));

            var result = ExecutionResultExtensions.Ok<IAsyncPagedEnumerable<int>>(paged).Build();
            return Task.FromResult(result);
        }
    }

    [Fact]
    public async Task Mediator_StreamWithPagination_ShouldReturnCorrectItems()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<PagedStreamRequest>, PagedStreamHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        ExecutionResult<IAsyncPagedEnumerable<int>> result = await mediator.SendAsync(
            new PagedStreamRequest { PageSize = 5, TotalItems = 15 });

        var items = new List<int>();
        await foreach (var item in result.Value!)
        {
            items.Add(item);
        }

        // Assert
        items.Should().HaveCount(15);
        items.Should().BeInAscendingOrder();

        var pagination = await result.Value!.GetPaginationAsync();
        pagination.TotalCount.Should().Be(15);
    }

    // ========== Error Propagation ==========
    private sealed class ErrorPropagationRequest : IRequest { }

    private sealed class ErrorPropagationPreHandler : IRequestPreHandler<ErrorPropagationRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            RequestContext<ErrorPropagationRequest> context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ExecutionResultExtensions
                .Failure(HttpStatusCode.Unauthorized)
                .WithDetail("Unauthorized access")
                .WithError("AUTH_ERROR", "User not authenticated")
                .Build());
        }
    }

    private sealed class ErrorPropagationHandler : IRequestHandler<ErrorPropagationRequest>
    {
        public static bool WasCalled = false;

        public Task<ExecutionResult> HandleAsync(
            ErrorPropagationRequest request,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.FromResult(ExecutionResultExtensions.Ok().Build());
        }
    }

    [Fact]
    public async Task Mediator_PreHandlerError_ShouldPropagateAndSkipMainHandler()
    {
        // Arrange
        ErrorPropagationHandler.WasCalled = false;
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelinePreDecorator();
        services.AddTransient<IRequestPreHandler<ErrorPropagationRequest>, ErrorPropagationPreHandler>();
        services.AddTransient<IRequestHandler<ErrorPropagationRequest>, ErrorPropagationHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new ErrorPropagationRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        result.Detail.Should().Be("Unauthorized access");
        result.Errors.Should().Contain(e => e.Key == "AUTH_ERROR");
        ErrorPropagationHandler.WasCalled.Should().BeFalse();
    }

    // ========== Multiple Exception Handlers ==========
    private sealed class MultiExceptionRequest : IRequest { }

    private sealed class MultiExceptionHandler : IRequestHandler<MultiExceptionRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            MultiExceptionRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Test exception");
    }

    private sealed class FirstExceptionHandler : IRequestExceptionHandler<MultiExceptionRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            RequestContext<MultiExceptionRequest> context,
            Exception exception,
            CancellationToken cancellationToken = default)
        {
            context["firstHandled"] = true;
            // Let it propagate to next handler
            throw exception;
        }
    }

    private sealed class SecondExceptionHandler : IRequestExceptionHandler<MultiExceptionRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            RequestContext<MultiExceptionRequest> context,
            Exception exception,
            CancellationToken cancellationToken = default)
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            context.TryGetItem("firstHandled", out object? first).Should().BeTrue();
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            return Task.FromResult(ExecutionResultExtensions
                .Failure(HttpStatusCode.InternalServerError)
                .WithDetail(exception.Message)
                .WithError("HANDLED", "Exception was handled by second handler")
                .Build());
        }
    }

    [Fact]
    public async Task Mediator_MultipleExceptionHandlers_ShouldChainProperly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelineExceptionDecorator();
        services.AddTransient<IRequestExceptionHandler<MultiExceptionRequest>, FirstExceptionHandler>();
        services.AddTransient<IRequestExceptionHandler<MultiExceptionRequest>, SecondExceptionHandler>();
        services.AddTransient<IRequestHandler<MultiExceptionRequest>, MultiExceptionHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new MultiExceptionRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Key == "HANDLED");
    }

    // ========== Generic Response with Transformation ==========
    private sealed class TransformRequest : IRequest<string>
    {
        public required int Number { get; init; }
    }

    private sealed class TransformHandler : IRequestHandler<TransformRequest, string>
    {
        public Task<ExecutionResult<string>> HandleAsync(
            TransformRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = $"Number is: {request.Number}";
            return Task.FromResult(ExecutionResultExtensions.Ok(result).Build());
        }
    }

    private sealed class TransformPostHandler : IRequestPostHandler<TransformRequest>
    {
        public Task<ExecutionResult> HandleAsync(
            RequestContext<TransformRequest> context,
            ExecutionResult response,
            CancellationToken cancellationToken = default)
        {
            // Transform the response - get the value and uppercase it
            var typedResponse = response.ToExecutionResult<string>();
            if (typedResponse.IsSuccess && typedResponse.Value != null)
            {
                var transformed = typedResponse.Value.ToUpper();
                return Task.FromResult<ExecutionResult>(ExecutionResultExtensions.Ok(transformed).Build());
            }
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task Mediator_PostHandlerTransformation_ShouldModifyResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelinePostDecorator();
        services.AddTransient<IRequestHandler<TransformRequest>, TransformHandler>();
        services.AddTransient<IRequestPostHandler<TransformRequest>, TransformPostHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        ExecutionResult<string> result = await mediator.SendAsync(new TransformRequest { Number = 42 });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("NUMBER IS: 42");
    }

    // ========== Concurrent Requests ==========
    private sealed class ConcurrentRequest : IRequest<int>
    {
        public required int Id { get; init; }
    }

    private sealed class ConcurrentHandler : IRequestHandler<ConcurrentRequest, int>
    {
        private static int _counter = 0;

        public async Task<ExecutionResult<int>> HandleAsync(
            ConcurrentRequest request,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken);
            var value = Interlocked.Increment(ref _counter);
            return ExecutionResultExtensions.Ok(value).Build();
        }
    }

    [Fact]
    public async Task Mediator_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<ConcurrentRequest>, ConcurrentHandler>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var tasks = Enumerable.Range(1, 10)
            .Select(async i =>
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                ExecutionResult<int> result = await mediator.SendAsync(new ConcurrentRequest { Id = i });
                return result;
            });

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        results.Select(r => r.Value).Should().OnlyHaveUniqueItems();
    }
}
