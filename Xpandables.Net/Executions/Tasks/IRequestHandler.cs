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

using System.ComponentModel;

namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Represents a handler to process a request of type <typeparamref name="TRequest" />.
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
public interface IRequestHandler<in TRequest>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Handles an asynchronous operation based on the provided request and returns the result of the execution.
    /// </summary>
    /// <param name="request">The input data required to perform the operation.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the operation if needed.</param>
    /// <returns>An asynchronous task that yields the result of the execution.</returns>
    Task<ExecutionResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for processing requests after they have been executed.
/// </summary>
/// <typeparam name="TRequest">The type of request to be handled. 
/// Must implement the <see cref="IRequest"/> interface.</typeparam>
public interface IRequestPostHandler<TRequest>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Asynchronously handles the specified request after it has been executed.
    /// </summary>
    /// <param name="context">The context of the request, containing necessary information for processing.</param>
    /// <param name="response">The initial execution result to be modified or used during processing.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests, allowing the operation to be cancelled.</param>
    /// <returns>A task representing the asynchronous operation, containing the final execution result after processing the
    /// request.</returns>
    Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        ExecutionResult response,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a contract for handling a request asynchronously before it is processed.
/// </summary>
/// <remarks>Implementations of this interface are responsible for performing any necessary pre-processing on the
/// request before it is passed to the main processing logic. This can include validation, logging, or other preparatory
/// tasks.</remarks>
/// <typeparam name="TRequest">The type of the request to be handled. Must implement <see cref="IRequest"/>.</typeparam>
public interface IRequestPreHandler<TRequest>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Asynchronously handles the specified request before it is processed.
    /// </summary>
    /// <param name="context">The context to be processed. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ExecutionResult"/> of
    /// the request processing.</returns>
    Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for managing exceptions that occur during the processing of a request.
/// </summary>
/// <typeparam name="TRequest">The type of the request being processed. Must implement <see cref="IRequest"/>.</typeparam>
public interface IRequestExceptionHandler<TRequest>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Handles exceptions that occur during the processing of the specified request.
    /// </summary>
    /// <param name="context">The context of the request being processed.</param>
    /// <param name="exception">The exception that occurred during processing.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the execution result of type
    /// <see cref="ExecutionResult"/>.</returns>
    Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        Exception exception,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for processing requests of type <typeparamref name="TRequest"/> and returning a result of type
/// <typeparamref name="TResponse"/>.
/// </summary>
/// <remarks>This interface extends <see cref="IRequestHandler{TRequest}"/> to provide a mechanism for handling
/// requests that produce a response. Implementations should ensure that the <see cref="HandleAsync"/> method is
/// thread-safe and can handle concurrent requests if necessary.</remarks>
/// <typeparam name="TRequest">The type of request to be handled. Must implement <see cref="IRequest"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response returned after processing the request.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest>
    where TRequest : class, IRequest<TResponse>
{
    /// <summary>
    /// Asynchronously handles the specified request and returns the execution result.
    /// </summary>
    /// <param name="request">The request to be processed. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. 
    /// The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the execution result of type
    /// <typeparamref name="TResponse"/>.</returns>
    new Task<ExecutionResult<TResponse>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);

    async Task<ExecutionResult> IRequestHandler<TRequest>.HandleAsync(
        TRequest request, CancellationToken cancellationToken) =>
        await HandleAsync(request, cancellationToken).ConfigureAwait(false);
}

/// <summary>
/// Defines a handler for processing stream requests asynchronously, producing a stream of responses.
/// </summary>
/// <remarks>This interface extends <see cref="IRequestHandler{TRequest}"/> to support handling requests that
/// result in a stream of responses. Implementations should ensure that the stream is properly disposed of and that any
/// necessary cleanup is performed.</remarks>
/// <typeparam name="TRequest">The type of the request message.</typeparam>
/// <typeparam name="TResponse">The type of the response message.</typeparam>
public interface IStreamRequestHandler<in TRequest, TResponse> : IRequestHandler<TRequest>
    where TRequest : class, IStreamRequest<TResponse>
{
    /// <summary>
    /// Asynchronously handles the specified request and returns a stream of responses.
    /// </summary>
    /// <param name="request">The request to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation, containing a stream of responses.</returns>
    new Task<ExecutionResult<IAsyncEnumerable<TResponse>>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<ExecutionResult> IRequestHandler<TRequest>.HandleAsync(
        TRequest request, CancellationToken cancellationToken) =>
        await HandleAsync(request, cancellationToken).ConfigureAwait(false);
}

/// <summary>
/// Defines a handler for a request of type <typeparamref name="TRequest" />
/// with a dependency of type <typeparamref name="TDependency" />.
/// </summary>
/// <remarks>This can also be enhanced with some useful decorators.</remarks>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public interface IDependencyRequestHandler<in TRequest, in TDependency> : IRequestHandler<TRequest>
    where TRequest : class, IDependencyRequest<TDependency>
    where TDependency : class
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<ExecutionResult> IRequestHandler<TRequest>.HandleAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DependencyInstance is null)
        {
            throw new InvalidOperationException("The dependency is not set.");
        }

        return await HandleAsync(request, (TDependency)request.DependencyInstance, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the specified request asynchronously with the given dependency.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="dependency">The dependency required to handle the request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the operation result.
    /// </returns>
    Task<ExecutionResult> HandleAsync(
        TRequest request,
        TDependency dependency,
        CancellationToken cancellationToken = default);
}