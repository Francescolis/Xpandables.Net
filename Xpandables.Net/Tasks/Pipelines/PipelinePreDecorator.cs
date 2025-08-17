
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
using Xpandables.Net.Executions;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Tasks.Pipelines;

/// <summary>
/// Represents a decorator that executes a series of pre-handlers before the main request handler in a pipeline.
/// </summary>
/// <remarks>This decorator iterates over a collection of pre-handlers, executing each one in sequence before
/// invoking the main request handler. It is used to perform operations or checks that should occur before the main
/// request processing.</remarks>
/// <typeparam name="TRequest">The type of the request being processed. Must implement <see cref="IRequest"/>.</typeparam>
/// <param name="preHandlers"></param>
public sealed class PipelinePreDecorator<TRequest>(
    IEnumerable<IRequestPreHandler<TRequest>> preHandlers) : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    ///<inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken)
    {
        foreach (IRequestPreHandler<TRequest> preHandler in preHandlers)
        {
            ExecutionResult result = await preHandler
                .HandleAsync(context, cancellationToken)
                .ConfigureAwait(false);

            // If any pre-handler returns a failure response, we short-circuit and return that response.
            if (!result.IsSuccessStatusCode)
            {
                return result;
            }
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}
