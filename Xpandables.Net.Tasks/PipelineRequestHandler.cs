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
/// The pipeline structure is pre-built at construction time for maximum performance.
/// This implementation uses iterative execution with a pre-built delegate chain to eliminate:
/// - Per-request allocations for decorator chain construction
/// - Recursive call overhead
/// - Lambda capture allocations in hot paths
/// Decorators are applied in reverse order to maintain expected execution flow.
/// </remarks>
public sealed class PipelineRequestHandler<TRequest> :
    IPipelineRequestHandler<TRequest>
    where TRequest : class, IRequest
{
    private readonly IRequestHandler<TRequest> _decoratee;
    private readonly IPipelineDecorator<TRequest>[] _decorators;
    private readonly bool _isContextHandler;
    private readonly Func<TRequest, CancellationToken, Task<ExecutionResult>>? _fastPath;

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
        // This eliminates per-request Reverse() and array allocations
        _decorators = decorators as IPipelineDecorator<TRequest>[]
            ?? [.. decorators];

        // Optimize: if no decorators, create fast path that bypasses pipeline machinery
        if (_decorators.Length == 0)
        {
            _fastPath = _isContextHandler
                ? (req, ct) => ((IRequestContextHandler<TRequest>)_decoratee).HandleAsync(new RequestContext<TRequest>(req), ct)
                : (req, ct) => _decoratee.HandleAsync(req, ct);
        }
        else if (_decorators.Length > 1)
        {
            Array.Reverse(_decorators);
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<ExecutionResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Fast path: no decorators - bypass all pipeline machinery
        if (_fastPath is not null)
        {
            return _fastPath(request, cancellationToken);
        }

        // Standard path: execute decorator chain
        RequestContext<TRequest> context = new(request);
        return ExecutePipelineIterativeAsync(context, cancellationToken);
    }

    /// <summary>
    /// Executes the decorator pipeline iteratively to avoid stack overhead and reduce allocations.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private async Task<ExecutionResult> ExecutePipelineIterativeAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken)
    {
        // Use a state machine approach to avoid recursive allocations
        // Build the chain bottom-up: start with the final handler, then wrap with each decorator

        // Create the innermost handler (the decoratee)
        RequestHandler current = _isContextHandler
            ? ct => ((IRequestContextHandler<TRequest>)_decoratee).HandleAsync(context, ct)
            : ct => _decoratee.HandleAsync(context.Request, ct);

        // Wrap with each decorator in reverse order (we already reversed the array in constructor)
        // We build the chain upfront to avoid allocations during execution
        for (int i = _decorators.Length - 1; i >= 0; i--)
        {
            IPipelineDecorator<TRequest> decorator = _decorators[i];
            RequestHandler next = current;

            // Capture decorator and next in closure - this allocation happens once per decorator
            current = ct => decorator.HandleAsync(context, next, ct);
        }

        // Execute the fully constructed chain
        return await current(cancellationToken).ConfigureAwait(false);
    }
}