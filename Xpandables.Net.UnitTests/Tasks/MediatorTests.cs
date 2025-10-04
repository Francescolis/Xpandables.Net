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
using System.Net.DependencyInjection;
using System.Net.ExecutionResults;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Tasks;

namespace Xpandables.Net.UnitTests.Tasks;

public class MediatorTests
{
    private sealed class Ping : IRequest { }

    private sealed class PingHandler : IRequestHandler<Ping>
    {
        public Task<ExecutionResult> HandleAsync(Ping request, CancellationToken cancellationToken = default)
            => Task.FromResult(ExecutionResultExtensions.Ok().Build());
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
}
