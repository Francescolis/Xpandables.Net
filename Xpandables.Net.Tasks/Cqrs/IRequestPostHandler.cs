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
using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net.Tasks.Cqrs;

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
