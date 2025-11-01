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
