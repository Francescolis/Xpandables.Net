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
using System.ComponentModel;

namespace System.Results.Requests;

/// <summary>
/// Represents a context-aware handler that can process a request of type <typeparamref name="TRequest" />
/// with access to the full request context, including contextual information set by pipeline decorators,
/// pre-handlers, or other components.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <remarks>
/// This interface extends <see cref="IRequestHandler{TRequest}"/> to provide access to the request context.
/// Handlers implementing this interface will receive the context version when called from the pipeline,
/// allowing them to access and modify contextual information throughout the request processing lifecycle.
/// </remarks>
public interface IRequestContextHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Handles an asynchronous operation based on the provided request context and returns the result of the execution.
    /// This method provides access to the full request context, allowing handlers to access contextual information
    /// set by pipeline decorators, pre-handlers, or other components.
    /// </summary>
    /// <param name="context">The request context containing the request and additional contextual information.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the operation if needed.</param>
    /// <returns>An asynchronous task that yields the result of the execution.</returns>
    Task<Result> HandleAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task<Result> IRequestHandler<TRequest>.HandleAsync(
        TRequest request, CancellationToken cancellationToken) =>
        HandleAsync(new(request), cancellationToken);
}

/// <summary>
/// Defines a handler for processing requests within a specified <see cref="RequestContext{TRequest}"/>.
/// </summary>
/// <remarks>This interface extends <see cref="IRequestHandler{TRequest, TResponse}"/> to provide additional
/// context-aware handling capabilities. Implementations should use the provided <see cref="RequestContext{TRequest}"/>
/// to access contextual information about the request, such as metadata or dependencies.</remarks>
/// <typeparam name="TRequest">The type of the request being handled. Must implement <see cref="IRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
public interface IRequestContextHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request asynchronously and returns the result of the execution.
    /// </summary>
    /// <remarks>This method processes the request encapsulated in the <paramref name="context"/> and produces
    /// a response of type <typeparamref name="TResponse"/>. The caller can use the <paramref name="cancellationToken"/>
    /// to cancel the operation if needed.</remarks>
    /// <param name="context">The context of the request, containing the request data and any associated metadata.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see
    /// cref="Result{TResponse}"/> representing the outcome of the request execution.</returns>
    Task<Result<TResponse>> HandleAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<Result<TResponse>> IRequestHandler<TRequest, TResponse>.HandleAsync(
        TRequest request, CancellationToken cancellationToken) =>
        await HandleAsync(new(request), cancellationToken).ConfigureAwait(false);
}

/// <summary>
/// Defines a handler for processing stream-based requests within a specific request context.
/// </summary>
/// <remarks>This interface extends <see cref="IStreamRequestHandler{TRequest, TResponse}"/> by providing
/// additional context-aware handling capabilities through the <see cref="RequestContext{TRequest}"/>.</remarks>
/// <typeparam name="TRequest">The type of the request being handled. Must implement <see cref="IStreamRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response elements produced by the stream.</typeparam>
public interface IStreamRequestContextHandler<TRequest, TResponse> : IStreamRequestHandler<TRequest, TResponse>
    where TRequest : class, IStreamRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request asynchronously and returns the result of the operation.
    /// </summary>
    /// <remarks>This method processes the request asynchronously and supports streaming responses via <see
    /// cref="IAsyncEnumerable{T}"/>. The caller can enumerate the responses as they are produced.</remarks>
    /// <param name="context">The request context containing the input data and metadata required to process the request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Result{T}"/>
    /// wrapping an <see cref="IAsyncEnumerable{T}"/> of <typeparamref name="TResponse"/> objects, representing the
    /// responses generated by the handler.</returns>
    Task<Result<IAsyncPagedEnumerable<TResponse>>> HandleAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<Result<IAsyncPagedEnumerable<TResponse>>> IStreamRequestHandler<TRequest, TResponse>.HandleAsync(
        TRequest request, CancellationToken cancellationToken) =>
        await HandleAsync(new(request), cancellationToken).ConfigureAwait(false);
}