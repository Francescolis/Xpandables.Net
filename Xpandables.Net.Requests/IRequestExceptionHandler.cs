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

namespace Xpandables.Net.Requests;

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
