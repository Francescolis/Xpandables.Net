
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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Distribution;

/// <summary>
/// Represents a method signature to be used to apply 
/// <see cref="IAsyncRequestHandler{TRequest, TResponse}"/> implementation.
/// </summary>
/// <typeparam name="TRequest">Type of the request that will be used as argument
/// .</typeparam>
/// <typeparam name="TResponse">Type of the response of the request.</typeparam>
/// <param name="request">The request to act on.</param>
/// <param name="cancellationToken">A CancellationToken 
/// to observe while waiting for the task to complete.</param>
/// <returns>An enumerator of <typeparamref name="TResponse"/> 
/// that can be asynchronously enumerated.</returns>
/// <exception cref="OperationResultException">The operation failed.</exception>
/// <exception cref="InvalidOperationException">
/// Unable to execute the process.</exception>
public delegate IAsyncEnumerable<TResponse> AsyncRequestHandler<TRequest, out TResponse>(
    TRequest request,
    CancellationToken cancellationToken = default)
    where TRequest : notnull, IAsyncRequest<TResponse>;

/// <summary>
/// Defines a generic method that a class implements to asynchronously handle a 
/// type-specific request and returns an asynchronous enumerable type-specific 
/// response.
/// The implementation must be thread-safe when working 
/// in a multi-threaded environment.
/// </summary>
/// <typeparam name="TRequest">Type of the request that 
/// will be used as argument.</typeparam>
/// <typeparam name="TResponse">Type of the response of the request.</typeparam>
public interface IAsyncRequestHandler<in TRequest, out TResponse>
    where TRequest : notnull, IAsyncRequest<TResponse>
{
    /// <summary>
    /// Asynchronously handles the specified request and returns
    /// an asynchronous enumerable of specific-type.
    /// </summary>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">
    /// The <paramref name="request"/> is null.</exception>
    /// <exception cref="OperationResultException">
    /// The operation failed.</exception>
    /// <exception cref="InvalidOperationException">
    /// The operation failed. See inner exception.</exception>
    /// <returns>An enumerator of <typeparamref name="TResponse"/> 
    /// that can be asynchronously enumerated.</returns>
    IAsyncEnumerable<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
}