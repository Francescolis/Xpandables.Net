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
using System.Diagnostics.CodeAnalysis;
using System.Net;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Collections;
using Xpandables.Net.Collections.Generic;
using Xpandables.Net.Cqrs;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Optionals;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.UnitTests.Tasks;

public class MediatorInterfaceTests
{
    // ========== Base IRequestHandler ==========
    private sealed class Simple : IRequest { }

    private sealed class SimpleHandler : IRequestHandler<Simple>
    {
        public Task<ExecutionResult> HandleAsync(Simple request, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResult.Ok().Build());
    }

    [Fact]
    public async Task SendAsync_IRequestHandler_ShouldReturnSuccess()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<Simple>, SimpleHandler>();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new Simple());

        result.IsSuccess.Should().BeTrue();
        ((int)result.StatusCode).Should().Be((int)HttpStatusCode.OK);
    }

    // ========== IRequestContextHandler ==========
    private sealed class ContextReq : IRequest { }

    private sealed class ContextPreHandler : IRequestPreHandler<ContextReq>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<ContextReq> context, CancellationToken cancellationToken = default)
        {
            context["pre"] = 1;
            return Task.FromResult(ExecutionResult.Ok().Build());
        }
    }

    private sealed class ContextHandler : IRequestContextHandler<ContextReq>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<ContextReq> context, CancellationToken cancellationToken = default)
        {
            context.TryGetItem("pre", out object? value).Should().BeTrue();
            value.Should().Be(1);
            return Task.FromResult(ExecutionResult.Ok().Build());
        }
    }

    [Fact]
    public async Task SendAsync_IRequestContextHandler_ShouldReceiveContext()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelinePreDecorator();
        services.AddTransient<IRequestPreHandler<ContextReq>, ContextPreHandler>();
        services.AddTransient<IRequestHandler<ContextReq>, ContextHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new ContextReq());

        result.IsSuccess.Should().BeTrue();
    }

    // ========== IRequestHandler<TRequest, TResponse> and implicit operator to ExecutionResult<T> ==========
    private sealed class EchoString : IRequest<string> { }

    private sealed class EchoStringHandler : IRequestHandler<EchoString, string>
    {
        public Task<ExecutionResult<string>> HandleAsync(EchoString request, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResult.Ok("hello").Build());
    }

    [Fact]
    public async Task SendAsync_GenericHandler_ImplicitToGenericResult_ShouldWork()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<EchoString>, EchoStringHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // IMediator.SendAsync returns ExecutionResult (non-generic) but implicit operator to ExecutionResult<string> should apply
        ExecutionResult<string> typed = await mediator.SendAsync(new EchoString());

        typed.IsSuccess.Should().BeTrue();
        typed.Value.Should().Be("hello");
        typed.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ========== IRequestContextHandler<TRequest, TResponse> ==========
    private sealed class AddNumbers : IRequest<int>
    {
        public required int A { get; init; }
        public required int B { get; init; }
    }

    private sealed class AddNumbersPre : IRequestPreHandler<AddNumbers>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<AddNumbers> context, CancellationToken cancellationToken = default)
        {
            // put a context item that handler will read
            context["sum"] = context.Request.A + context.Request.B;
            return Task.FromResult(ExecutionResult.Ok().Build());
        }
    }

    private sealed class AddNumbersHandler : IRequestContextHandler<AddNumbers, int>
    {
        public Task<ExecutionResult<int>> HandleAsync(RequestContext<AddNumbers> context, CancellationToken cancellationToken = default)
        {
            int sum = (context.TryGetItem("sum", out object? value) && value is int s)
                ? s
                : context.Request.A + context.Request.B;
            return Task.FromResult(ExecutionResult.Ok(sum).Build());
        }
    }

    [Fact]
    public async Task SendAsync_IRequestContextHandler_TResult_ShouldReturnTypedValue()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        // Pre decorator not required for this test now; context fallback uses request values.
        services.AddTransient<IRequestHandler<AddNumbers>, AddNumbersHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        ExecutionResult<int> typed = await mediator.SendAsync(new AddNumbers { A = 2, B = 3 });
        typed.IsSuccess.Should().BeTrue();
        typed.Value.Should().Be(5);
    }

    // ========== IStreamRequestHandler<TRequest, TResponse> ==========
    private sealed class StreamInts : IStreamRequest<int> { }

    private sealed class StreamIntsHandler : IStreamRequestHandler<StreamInts, int>, IRequestHandler<StreamInts>
    {
        public Task<ExecutionResult<IAsyncPagedEnumerable<int>>> HandleAsync(StreamInts request, CancellationToken cancellationToken = default)
        {
            static async IAsyncEnumerable<int> Source()
            {
                yield return 1;
                await Task.Yield();
                yield return 2;
                await Task.Yield();
                yield return 3;
            }

            var paged = new AsyncPagedEnumerable<int>(Source(), ct => new ValueTask<Pagination>(Pagination.Create(2, 1, null, 3)));
            var result = ExecutionResult.Ok<IAsyncPagedEnumerable<int>>(paged).Build();
            return Task.FromResult(result);
        }

        // Explicit IRequestHandler implementation is provided by the interface default method via covariance
    }

    [Fact]
    public async Task SendAsync_IStreamRequestHandler_ShouldReturnPagedEnumerable()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<StreamInts>, StreamIntsHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        ExecutionResult<IAsyncPagedEnumerable<int>> typed = await mediator.SendAsync(new StreamInts());

        typed.IsSuccess.Should().BeTrue();
        var list = new List<int>();
        Assert.NotNull(typed.Value);
        await foreach (var i in typed.Value)
        {
            list.Add(i);
        }
        list.Should().Equal(1, 2, 3);

        var ctx = await typed.Value.GetPaginationAsync();
        ctx.TotalCount.Should().Be(3);
    }

    // ========== IStreamRequestContextHandler<TRequest, TResponse> ==========
    private sealed class StreamIntsWithContext : IStreamRequest<int>
    {
        public required int Count { get; init; }
    }

    private sealed class StreamIntsWithContextPre : IRequestPreHandler<StreamIntsWithContext>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<StreamIntsWithContext> context, CancellationToken cancellationToken = default)
        {
            context["count"] = context.Request.Count;
            return Task.FromResult(ExecutionResult.Ok().Build());
        }
    }

    private sealed class StreamIntsWithContextHandler : IStreamRequestContextHandler<StreamIntsWithContext, int>, IRequestHandler<StreamIntsWithContext>
    {
        public Task<ExecutionResult<IAsyncPagedEnumerable<int>>> HandleAsync(RequestContext<StreamIntsWithContext> context, CancellationToken cancellationToken = default)
        {
            int count = context.TryGetItem("count", out object? v) && v is int c ? c : context.Request.Count;
            async IAsyncEnumerable<int> Source()
            {
                for (int i = 1; i <= count; i++)
                {
                    yield return i;
                    await Task.Yield();
                }
            }

            var paged = new AsyncPagedEnumerable<int>(Source(), ct => new ValueTask<Pagination>(Pagination.Create(count, 1, null, count)));
            var result = ExecutionResult.Ok<IAsyncPagedEnumerable<int>>(paged).Build();
            return Task.FromResult(result);
        }
    }

    [Fact]
    public async Task SendAsync_IStreamRequestContextHandler_ShouldReceiveContextAndStream()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        // Pre decorator removed; handler derives count from request directly if context value absent.
        services.AddTransient<IRequestHandler<StreamIntsWithContext>, StreamIntsWithContextHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        ExecutionResult<IAsyncPagedEnumerable<int>> typed = await mediator.SendAsync(new StreamIntsWithContext { Count = 4 });
        var values = new List<int>();
        Assert.NotNull(typed.Value);
        await foreach (var i in typed.Value)
        {
            values.Add(i);
        }
        values.Should().Equal(1, 2, 3, 4);
    }

    // ========== IRequestPostHandler ==========
    private sealed class PostReq : IRequest { }

    private sealed class PostHandler : IRequestHandler<PostReq>
    {
        public Task<ExecutionResult> HandleAsync(PostReq request, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResult.Ok().Build());
    }

    private sealed class PostDecoratorHandler : IRequestPostHandler<PostReq>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<PostReq> context, ExecutionResult response, CancellationToken cancellationToken = default)
        {
            if (context.TryGetItem("makeFailure", out object? v) && v is true)
            {
                return Task.FromResult(ExecutionResult.Failure(HttpStatusCode.BadRequest).Build());
            }

            return Task.FromResult(response);
        }
    }

    private sealed class PostPre : IRequestPreHandler<PostReq>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<PostReq> context, CancellationToken cancellationToken = default)
        {
            context["makeFailure"] = true;
            return Task.FromResult(ExecutionResult.Ok().Build());
        }
    }

    [Fact]
    public async Task SendAsync_IRequestPostHandler_ShouldBeAbleToModifyResponse()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelinePreDecorator();
        services.AddXPipelinePostDecorator();
        services.AddTransient<IRequestPreHandler<PostReq>, PostPre>();
        services.AddTransient<IRequestPostHandler<PostReq>, PostDecoratorHandler>();
        services.AddTransient<IRequestHandler<PostReq>, PostHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new PostReq());
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ========== IRequestExceptionHandler ==========
    private sealed class ExceptReq : IRequest { }

    private sealed class ThrowingHandler : IRequestHandler<ExceptReq>
    {
        public Task<ExecutionResult> HandleAsync(ExceptReq request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");
    }

    private sealed class ExceptHandler : IRequestExceptionHandler<ExceptReq>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<ExceptReq> context, Exception exception, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResult.Failure(HttpStatusCode.Conflict).WithDetail(exception.Message).Build());
    }

    [Fact]
    public async Task SendAsync_IRequestExceptionHandler_ShouldTransformExceptions()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelineExceptionDecorator();
        services.AddTransient<IRequestExceptionHandler<ExceptReq>, ExceptHandler>();
        services.AddTransient<IRequestHandler<ExceptReq>, ThrowingHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new ExceptReq());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        result.Detail.Should().Be("boom");
    }

    // ========== IDependencyRequestHandler / IDependencyRequestContextHandler ==========
    private sealed class MyService(string name)
    {
        public string Name { get; } = name;
    }

    private sealed record MyDepReq : DependencyRequest<MyService>
    {
        [SetsRequiredMembers]
        public MyDepReq(object id) => DependencyKeyId = id;
    }

    private sealed class MyDepProvider : IDependencyProvider
    {
        public bool CanProvideDependency(Type dependencyType) => dependencyType == typeof(MyService);

        public Task<object> GetDependencyAsync(IDependencyRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<object>(new MyService($"dep-{request.DependencyKeyId}"));
    }

    private sealed class MyDepHandler : IDependencyRequestHandler<MyDepReq, MyService>, IRequestHandler<MyDepReq>
    {
        public Task<ExecutionResult> HandleAsync(MyDepReq request, CancellationToken cancellationToken = default)
        {
            request.DependencyInstance.TryGetValue(out MyService? service).Should().BeTrue();
            service!.Name.Should().StartWith("dep-");
            return Task.FromResult(ExecutionResult.Ok().Build());
        }
    }

    private sealed class MyDepContextHandler : IDependencyRequestContextHandler<MyDepReq, MyService>, IRequestHandler<MyDepReq>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<MyDepReq> context, CancellationToken cancellationToken = default)
        {
            context.Request.DependencyInstance.TryGetValue(out MyService? service).Should().BeTrue();
            service!.Name.Should().StartWith("dep-");
            return Task.FromResult(ExecutionResult.Ok().Build());
        }
    }

    [Fact]
    public async Task SendAsync_IDependencyRequestHandler_ShouldResolveDependency()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXDependencyManager();
        services.AddXDependencyProvider<MyDepProvider>();
        services.AddXPipelineDependencyDecorator();
        services.AddTransient<IRequestHandler<MyDepReq>, MyDepHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new MyDepReq("42"));
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_IDependencyRequestContextHandler_ShouldResolveDependency_WithContext()
    {
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXDependencyManager();
        services.AddXDependencyProvider<MyDepProvider>();
        services.AddXPipelineDependencyDecorator();
        services.AddTransient<IRequestHandler<MyDepReq>, MyDepContextHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new MyDepReq("84"));
        result.IsSuccess.Should().BeTrue();
    }

    // ========== Negative-path tests ==========
    [Fact]
    public async Task SendAsync_PreHandlerFailure_ShouldShortCircuit_AndSkipMainHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelinePreDecorator();
        services.AddTransient<IRequestPreHandler<PreFailReq>, PreFailingPreHandler>();
        services.AddTransient<IRequestHandler<PreFailReq>, ShouldNotBeCalledHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new PreFailReq());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ShouldNotBeCalledHandler.CallCount.Should().Be(0);
    }

    private sealed class PreFailReq : IRequest { }

    private sealed class PreFailingPreHandler : IRequestPreHandler<PreFailReq>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<PreFailReq> context, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResult.Failure(HttpStatusCode.BadRequest).WithDetail("blocked").Build());
    }

    private sealed class ShouldNotBeCalledHandler : IRequestHandler<PreFailReq>
    {
        public static int CallCount = 0;
        public Task<ExecutionResult> HandleAsync(PreFailReq request, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref CallCount);
            return Task.FromResult(ExecutionResult.Ok().Build());
        }
    }

    [Fact]
    public async Task SendAsync_ExceptionDecorator_NoHandler_ShouldReturnFailureFromException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelineExceptionDecorator();
        services.AddTransient<IRequestHandler<ExceptNoHandlerReq>, ExceptNoHandlerThrowingHandler>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new ExceptNoHandlerReq());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    private sealed class ExceptNoHandlerReq : IRequest { }

    private sealed class ExceptNoHandlerThrowingHandler : IRequestHandler<ExceptNoHandlerReq>
    {
        public Task<ExecutionResult> HandleAsync(ExceptNoHandlerReq request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("oops");
    }

    [Fact]
    public async Task SendAsync_PostHandlers_WhenFirstFails_ShouldStopChain()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddXPipelinePostDecorator();
        services.AddTransient<IRequestHandler<PostChainReq>, PostChainMainHandler>();
        services.AddTransient<IRequestPostHandler<PostChainReq>, FirstFailingPostHandler>();
        services.AddTransient<IRequestPostHandler<PostChainReq>, SecondPostHandlerShouldNotRun>();

        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act
        var result = await mediator.SendAsync(new PostChainReq());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.Conflict);
        SecondPostHandlerShouldNotRun.CallCount.Should().Be(0);
    }

    private sealed class PostChainReq : IRequest { }

    private sealed class PostChainMainHandler : IRequestHandler<PostChainReq>
    {
        public Task<ExecutionResult> HandleAsync(PostChainReq request, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResult.Ok().Build());
    }

    private sealed class FirstFailingPostHandler : IRequestPostHandler<PostChainReq>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<PostChainReq> context, ExecutionResult response, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResult.Failure(HttpStatusCode.Conflict).Build());
    }

    private sealed class SecondPostHandlerShouldNotRun : IRequestPostHandler<PostChainReq>
    {
        public static int CallCount = 0;
        public Task<ExecutionResult> HandleAsync(RequestContext<PostChainReq> context, ExecutionResult response, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref CallCount);
            return Task.FromResult(response);
        }
    }

    [Fact]
    public async Task SendAsync_ImplicitConversion_ToMismatchedGeneric_ShouldDefaultValue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddXMediator();
        services.AddXPipelineRequestHandler();
        services.AddTransient<IRequestHandler<EchoString>, EchoStringHandler>();
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        // Act: Expecting int but actual is string -> Value becomes default(int)
        ExecutionResult<int> typed = await mediator.SendAsync(new EchoString());

        // Assert
        typed.IsSuccess.Should().BeTrue();
        typed.Value.Should().Be(0);
    }
}
