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

using System.Results;

namespace System.OperationResults.Tasks;

/// <summary>
/// Defines a contract for sending requests to handlers and receiving execution results asynchronously.
/// </summary>
/// <remarks>Implementations of this interface coordinate the dispatch of request objects to their corresponding
/// handlers. This abstraction enables decoupling between request senders and handlers, supporting patterns such as CQRS
/// and request/response messaging. Thread safety and handler registration are implementation-specific and may
/// vary.</remarks>
public interface IMediator
{
    /// <summary>
    /// Sends the specified request asynchronously and returns the result of its execution.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to send. Must implement the <see cref="IRequest"/> interface.</typeparam>
    /// <param name="request">The request object to be sent. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation. The task result contains the execution outcome of the
    /// request.</returns>
    /// <exception cref="ResultException">Thrown when the execution result indicates a failure.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="request"/> is null.</exception>
    Task<OperationResult> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IRequest;
}
