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

using Xpandables.Net.Executions.Pipelines;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// A wrapper for applying pipeline on requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public sealed class PipelineRequestHandler<TRequest>(
    IRequestHandler<TRequest> decoratee,
    IEnumerable<IPipelineDecorator<TRequest, ExecutionResult>> decorators) :
    IPipelineRequestHandler<TRequest>
    where TRequest : class, IRequest
{
    /// <inheritdoc />
    public async Task<ExecutionResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        ExecutionResult result = await decorators
            .Reverse()
            .Aggregate<IPipelineDecorator<TRequest, ExecutionResult>,
                RequestHandler<ExecutionResult>>(
                Handler,
                (next, decorator) => async () => await decorator.HandleAsync(
                    request,
                    next,
                    cancellationToken).ConfigureAwait(false))().ConfigureAwait(false);

        return result;

        async Task<ExecutionResult> Handler()
        {
            return await decoratee.HandleAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}