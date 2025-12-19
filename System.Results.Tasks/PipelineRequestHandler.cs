/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Results.Pipelines;
using System.Results.Requests;
using System.Runtime.CompilerServices;

namespace System.Results.Tasks;

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
    private readonly bool _isContextHandler;
    private readonly Func<TRequest, CancellationToken, Task<Result>>? _fastPath;
    private readonly Func<RequestContext<TRequest>, CancellationToken, Task<Result>>? _pipeline;

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

        // Materialize decorators once at construction time to avoid per-request allocations.
        IPipelineDecorator<TRequest>[] decoratorArray = decorators as IPipelineDecorator<TRequest>[]
            ?? [.. decorators];

        // Optimize: if no decorators, create fast path that bypasses pipeline machinery
        if (decoratorArray.Length == 0)
        {
            _fastPath = _isContextHandler
                ? (req, ct) => ((IRequestContextHandler<TRequest>)_decoratee).HandleAsync(new RequestContext<TRequest>(req), ct)
                : (req, ct) => _decoratee.HandleAsync(req, ct);
        }
        else
        {
            if (decoratorArray.Length > 1)
            {
                Array.Reverse(decoratorArray);
            }

            _pipeline = BuildPipeline(decoratorArray);
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<Result> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Fast path: no decorators - bypass all pipeline machinery
        if (_fastPath is not null)
        {
            return _fastPath(request, cancellationToken);
        }

        // Standard path: execute pre-built pipeline
        RequestContext<TRequest> context = new(request);
        if (_pipeline is null)
        {
            throw new InvalidOperationException("Pipeline execution delegate is not configured.");
        }

        return _pipeline(context, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Func<RequestContext<TRequest>, CancellationToken, Task<Result>> BuildPipeline(
        IPipelineDecorator<TRequest>[] decorators)
    {
        Func<RequestContext<TRequest>, CancellationToken, Task<Result>> current = _isContextHandler
            ? (ctx, ct) => ((IRequestContextHandler<TRequest>)_decoratee).HandleAsync(ctx, ct)
            : (ctx, ct) => _decoratee.HandleAsync(ctx.Request, ct);

        for (int i = decorators.Length - 1; i >= 0; i--)
        {
            IPipelineDecorator<TRequest> decorator = decorators[i];
            Func<RequestContext<TRequest>, CancellationToken, Task<Result>> next = current;

            current = (ctx, ct) =>
            {
#pragma warning disable IDE0039 // Use local function
                RequestHandler nextHandler = cancellationToken => next(ctx, cancellationToken);
#pragma warning restore IDE0039 // Use local function
                return decorator.HandleAsync(ctx, nextHandler, ct);
            };
        }

        return current;
    }
}