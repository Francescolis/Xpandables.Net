
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

namespace Xpandables.Net.Tasks.Pipelines;

/// <summary>
/// Represents a decorator that executes post-handling logic for a request in a pipeline.
/// </summary>
/// <remarks>This decorator is used to apply additional processing after the main request handler has executed. It
/// iterates over a collection of post-handlers, invoking each one with the request context and the result of the main
/// handler. This allows for operations such as logging, auditing, or modifying the response.</remarks>
/// <typeparam name="TRequest">The type of the request being processed. Must implement <see cref="IRequest"/>.</typeparam>
/// <param name="postHandlers"></param>
public sealed class PipelinePostDecorator<TRequest>(
    IEnumerable<IRequestPostHandler<TRequest>> postHandlers) : IPipelineDecorator<TRequest>
    where TRequest : class, IRequest
{
    ///<inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        RequestHandler next,
        CancellationToken cancellationToken)
    {
        ExecutionResult response = await next(cancellationToken).ConfigureAwait(false);

        foreach (IRequestPostHandler<TRequest> postHandler in postHandlers)
        {
            ExecutionResult result = await postHandler
                .HandleAsync(context, response, cancellationToken)
                .ConfigureAwait(false);

            // If any post-handler returns a failure response, we short-circuit and return that response.
            if (!result.IsSuccessStatusCode)
            {
                return result;
            }
        }

        return response;
    }
}
