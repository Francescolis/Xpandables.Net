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
/// An interface for handling asynchronous operations based on a specific request type.
/// </summary>
/// <typeparam name="TRequest">Represents the input data required to perform the operation, 
/// constrained to a class implementing a request interface.</typeparam>
public interface IPipelineRequestHandler<TRequest>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Handles an asynchronous operation based on the provided request and returns the result of the execution.
    /// </summary>
    /// <param name="request">The input data required to perform the operation.</param>
    /// <param name="cancellationToken">Used to signal the cancellation of the operation if needed.</param>
    /// <returns>An asynchronous task that yields the result of the execution.</returns>
    Task<ExecutionResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
}