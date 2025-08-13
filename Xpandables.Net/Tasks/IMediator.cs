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
using Xpandables.Net.Events;
using Xpandables.Net.Executions;

namespace Xpandables.Net.Tasks;

/// <summary>
/// Defines a mediator interface for sending requests and receiving execution results asynchronously.
/// </summary>
public interface IMediator : IPublisher
{
    /// <summary>
    /// Sends a request asynchronously and returns an execution result.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to be sent. This type must implement the <see cref="IRequest"/> interface.</typeparam>
    /// <param name="request">The request containing the data necessary to perform the operation.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, containing the execution result.</returns>
    Task<ExecutionResult> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IRequest;
}
