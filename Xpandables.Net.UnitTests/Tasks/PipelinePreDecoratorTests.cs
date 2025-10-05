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

using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Tasks;
using Xpandables.Net.Tasks.Pipelines;

namespace Xpandables.Net.UnitTests.Tasks;

public class PipelinePreDecoratorTests
{
    private sealed class TestRequest : IRequest { }

    private sealed class SuccessPreHandler : IRequestPreHandler<TestRequest>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<TestRequest> context, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResultExtensions.Ok().Build());
    }

    private sealed class FailurePreHandler : IRequestPreHandler<TestRequest>
    {
        public Task<ExecutionResult> HandleAsync(RequestContext<TestRequest> context, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResultExtensions.BadRequest().Build());
    }

    [Fact]
    public async Task HandleAsync_WhenAllPreHandlersSucceed_ShouldCallNext()
    {
        // Arrange
        var decorator = new PipelinePreDecorator<TestRequest>([new SuccessPreHandler(), new SuccessPreHandler()]);
        var context = new RequestContext<TestRequest>(new());

        var nextCalled = false;
        Task<ExecutionResult> Next(CancellationToken ct)
        {
            nextCalled = true;
            return Task.FromResult(ExecutionResultExtensions.Created().Build());
        }

        // Act
        var result = await decorator.HandleAsync(context, Next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenAPreHandlerFails_ShouldShortCircuit()
    {
        // Arrange: first succeeds, second fails, third should not run
        var preHandlers = new IRequestPreHandler<TestRequest>[]
        {
            new SuccessPreHandler(),
            new FailurePreHandler(),
            new SuccessPreHandler()
        };
        var decorator = new PipelinePreDecorator<TestRequest>(preHandlers);
        var context = new RequestContext<TestRequest>(new());

        var nextCalled = false;
        Task<ExecutionResult> Next(CancellationToken ct)
        {
            nextCalled = true;
            return Task.FromResult(ExecutionResultExtensions.Ok().Build());
        }

        // Act
        var result = await decorator.HandleAsync(context, Next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccess.Should().BeFalse();
    }
}
