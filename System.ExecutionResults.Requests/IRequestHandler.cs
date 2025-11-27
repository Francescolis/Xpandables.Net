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
namespace System.ExecutionResults;

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
    Task<OperationResult> HandleAsync(
        TRequest request,
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
    new Task<OperationResult<TResponse>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);

    async Task<OperationResult> IRequestHandler<TRequest>.HandleAsync(
        TRequest request, CancellationToken cancellationToken) =>
        await HandleAsync(request, cancellationToken).ConfigureAwait(false);
}