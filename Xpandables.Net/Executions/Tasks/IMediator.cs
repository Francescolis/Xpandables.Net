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
namespace Xpandables.Net.Executions.Tasks;

/// <summary>
/// Defines a mediator interface for sending requests and queries.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request asynchronously and returns the result of the execution.
    /// </summary>
    /// <typeparam name="TRequest">Represents the type of the request being sent, which must 
    /// implement a specific interface.</typeparam>
    /// <param name="request">Contains the data needed to perform the operation specified by the request.</param>
    /// <param name="cancellationToken">Allows the operation to be canceled if needed.</param>
    /// <returns>Provides the outcome of the execution as an asynchronous result.</returns>
    Task<ExecutionResult> SendAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : class, IRequest;
}
