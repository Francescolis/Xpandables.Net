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
using System.Runtime.CompilerServices;

using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net.Tasks;

/// <summary>
/// Represents a sealed class that implements the pipeline request handling mechanism for a given request type.
/// This handler uses a series of decorators, applied in reverse order, to process the request and execute the operation.
/// </summary>
/// <typeparam name="TRequest">The type of the request to be handled, which must implement <see cref="IRequest"/>.</typeparam>
/// <remarks>
/// The pipeline structure is constructed once at creation time and cached for optimal performance.
/// Decorators are applied in reverse order to maintain expected execution flow.
/// This implementation eliminates per-request allocations for decorator chain construction.
/// </remarks>
public sealed class PipelineRequestHandler<TRequest> :
    IPipelineRequestHandler<TRequest>
    where TRequest : class, IRequest
{
    private readonly IRequestHandler<TRequest> _decoratee;
    private readonly IPipelineDecorator<TRequest>[] _decorators;
    private readonly bool _isContextHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineRequestHandler{TRequest}"/> class.
    /// </summary>
    /// <param name="decoratee">The final handler in the pipeline that processes the request.</param>
    /// <param name="decorators">The collection of decorators to apply to the pipeline.</param>
    public PipelineRequestHandler(
        IRequestHandler<TRequest> decoratee,
        IEnumerable<IPipelineDecorator<TRequest>> decorators)
    {
        ArgumentNullException.ThrowIfNull(decoratee);
        ArgumentNullException.ThrowIfNull(decorators);

        _decoratee = decoratee;
        _isContextHandler = decoratee is IRequestContextHandler<TRequest>;

        // Materialize and reverse decorators once at construction time
        _decorators = decorators as IPipelineDecorator<TRequest>[]
            ?? [.. decorators];

        if (_decorators.Length > 1)
        {
            Array.Reverse(_decorators);
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<ExecutionResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        RequestContext<TRequest> context = new(request);

        ExecutionResult result = await ExecutePipelineAsync(context, 0, cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Recursively executes the decorator pipeline.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="index">Current decorator index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private Task<ExecutionResult> ExecutePipelineAsync(
        RequestContext<TRequest> context,
        int index,
        CancellationToken cancellationToken)
    {
        // If we've processed all decorators, invoke the final handler
        if (index >= _decorators.Length)
        {
            return _isContextHandler
                ? ((IRequestContextHandler<TRequest>)_decoratee).HandleAsync(context, cancellationToken)
                : _decoratee.HandleAsync(context.Request, cancellationToken);
        }

        // Get current decorator and create next handler
        IPipelineDecorator<TRequest> currentDecorator = _decorators[index];

        // Create the next handler that continues the chain
        RequestHandler nextHandler = (ct) => ExecutePipelineAsync(context, index + 1, ct);

        // Invoke current decorator
        return currentDecorator.HandleAsync(context, nextHandler, cancellationToken);
    }
}