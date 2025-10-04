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
using System.Net.ExecutionResults;
using System.Net.Tasks;

namespace System.Net.Tasks;

/// <summary>
/// Represents a sealed class that implements the pipeline request handling mechanism for a given request type.
/// This handler uses a series of decorators, applied in reverse order, to process the request and execute the operation.
/// </summary>
/// <typeparam name="TRequest">The type of the request to be handled, which must implement <see cref="IRequest"/>.</typeparam>
public sealed class PipelineRequestHandler<TRequest>(
    IRequestHandler<TRequest> decoratee,
    IEnumerable<IPipelineDecorator<TRequest>> decorators) :
    IPipelineRequestHandler<TRequest>
    where TRequest : class, IRequest
{
    /// <inheritdoc />
    public async Task<ExecutionResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        RequestContext<TRequest> context = new(request);

        ExecutionResult result = await decorators
            .Reverse()
            .Aggregate(
                (RequestHandler)Handler,
                (next, decorator) => async (ct) => await decorator.HandleAsync(
                    context,
                    next,
                    ct).ConfigureAwait(false))
            (cancellationToken).ConfigureAwait(false);

        return result;

        Task<ExecutionResult> Handler(CancellationToken ct) =>
            decoratee switch
            {
                IRequestContextHandler<TRequest> requestContextHandler =>
                    requestContextHandler.HandleAsync(context, ct),
                _ => decoratee.HandleAsync(request, ct)
            };
    }
}