
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

using Xpandables.Net.Aggregates;
using Xpandables.Net.Events;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Distribution;

/// <summary>
/// Defines a set of methods to automatically distribute requests and events.
/// The implementation must be 
/// thread-safe when working in a multi-threaded environment.
/// </summary>
public interface IDistributor : IServiceProvider, IEventPublisher
{
    /// <summary>
    /// Asynchronously send the request to the 
    /// <see cref="IRequestHandler{TRequest}"/> implementation handler.
    /// </summary>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <returns>A task that represents an <see cref="IOperationResult"/>
    /// .</returns>
    Task<IOperationResult> SendAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : notnull, IRequest;

    /// <summary>
    /// Asynchronously send the request to the 
    /// <see cref="IRequestAggregateHandler{TRequest, TAggregate}"/> 
    /// implementation handler.
    /// </summary>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <returns>A task that represents an <see cref="IOperationResult"/>
    /// .</returns>
    Task<IOperationResult> SendAsync<TRequest, TAggregate>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TAggregate : class, IAggregate
        where TRequest : class, IRequestAggregate<TAggregate>;

    /// <summary>
    /// Asynchronously gets the response of the request using
    /// the <see cref="IRequestHandler{TRequest, TResponse}"/> 
    /// implementation and returns a response.
    /// </summary>
    /// <typeparam name="TRequest">Type of the request</typeparam>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken to 
    /// observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <returns>A task that represents an 
    /// <see cref="IOperationResult{TValue}"/>.</returns>
    Task<IOperationResult<TResponse>> GetAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : notnull, IRequest<TResponse>;

    /// <summary>
    /// Asynchronously fetches the response from the request using
    /// the <see cref="IAsyncRequestHandler{TRequest, TResponse}"/> implementation
    /// and returns an enumerator of <typeparamref name="TResponse"/> 
    /// that can be asynchronously enumerated.
    /// </summary>
    /// <typeparam name="TRequest">Type of the request</typeparam>
    /// <typeparam name="TResponse">Type of the response.</typeparam>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <exception cref="OperationResultException">The operation failed
    /// .</exception>
    /// <returns>An enumerator of <typeparamref name="TResponse"/> 
    /// that can be asynchronously enumerated.</returns>
    IAsyncEnumerable<TResponse> FetchAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : notnull, IAsyncRequest<TResponse>;
}