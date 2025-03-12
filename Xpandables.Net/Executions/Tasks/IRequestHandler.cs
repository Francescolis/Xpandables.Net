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
/// Defines the base interface method to handle a request that returns a result.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IHandler<in TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    /// <summary>
    /// Handles the request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The response of the request.</returns>
    TResponse Handle(
        TRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for a request of type <typeparamref name="TRequest"/>.
/// </summary>
/// <remarks>This can also be enhanced with some useful decorators.</remarks>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public interface IRequestHandler<in TRequest> : IHandler<TRequest, Task<IExecutionResult>>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the operation result.</returns>
    Task<IExecutionResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1033 // Interface methods should be callable by child types
    Task<IExecutionResult> IHandler<TRequest, Task<IExecutionResult>>.Handle(
#pragma warning restore CA1033 // Interface methods should be callable by child types
        TRequest request,
        CancellationToken cancellationToken) =>
        HandleAsync(request, cancellationToken);
}

/// <summary>
/// Defines a handler for processing requests of type <typeparamref name="TRequest"/> 
/// and returning a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IRequestHandler<in TRequest, TResult> : IHandler<TRequest, Task<IExecutionResult<TResult>>>
    where TRequest : class, IRequest<TResult>
{
    /// <summary>
    /// Handles the specified query that returns a result asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task 
    /// result contains the operation result.</returns>
    Task<IExecutionResult<TResult>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1033 // Interface methods should be callable by child types
    Task<IExecutionResult<TResult>> IHandler<TRequest, Task<IExecutionResult<TResult>>>.Handle(
#pragma warning restore CA1033 // Interface methods should be callable by child types
        TRequest request,
        CancellationToken cancellationToken) =>
        HandleAsync(request, cancellationToken);
}

/// <summary>
/// Defines a handler for processing stream requests.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IStreamRequestHandler<in TRequest, TResult> : IHandler<TRequest, IAsyncEnumerable<TResult>>
    where TRequest : class, IStreamRequest<TResult>
    where TResult : notnull
{
    /// <summary>
    /// Handles the stream query.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of the result.</returns>
    IAsyncEnumerable<TResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CA1033 // Interface methods should be callable by child types
    IAsyncEnumerable<TResult> IHandler<TRequest, IAsyncEnumerable<TResult>>.Handle(
#pragma warning restore CA1033 // Interface methods should be callable by child types
        TRequest request,
        CancellationToken cancellationToken) =>
        HandleAsync(request, cancellationToken);
}

/// <summary>
/// Defines a handler for a request of type <typeparamref name="TRequest"/> 
/// with a dependency of type <typeparamref name="TDependency"/>.
/// </summary>
/// <remarks>This can also be enhanced with some useful decorators.</remarks>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public interface IDeciderRequestHandler<in TRequest, in TDependency> : IRequestHandler<TRequest>
    where TRequest : class, IDeciderRequest<TDependency>
    where TDependency : class
{
    /// <summary>
    /// Handles the specified request asynchronously with the given dependency.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="dependency">The dependency required to handle the request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the operation result.</returns>
    Task<IExecutionResult> HandleAsync(
        TRequest request,
        TDependency dependency,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task<IExecutionResult> IRequestHandler<TRequest>.HandleAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Dependency is null)
            throw new InvalidOperationException("The dependency is not set.");

        return HandleAsync(request, (TDependency)request.Dependency, cancellationToken);
    }
}
