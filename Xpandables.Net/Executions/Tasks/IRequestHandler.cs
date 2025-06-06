﻿/*******************************************************************************
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

using System.ComponentModel;

namespace Xpandables.Net.Executions.Tasks;

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
    Task<ExecutionResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for a request of type <typeparamref name="TRequest" />
/// with a dependency of type <typeparamref name="TDependency" />.
/// </summary>
/// <remarks>This can also be enhanced with some useful decorators.</remarks>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public interface IDependencyRequestHandler<in TRequest, in TDependency> : IRequestHandler<TRequest>
    where TRequest : class, IDependencyRequest<TDependency>
    where TDependency : class
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<ExecutionResult> IRequestHandler<TRequest>.HandleAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DependencyInstance is null)
        {
            throw new InvalidOperationException("The dependency is not set.");
        }

        return await HandleAsync(request, (TDependency)request.DependencyInstance, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the specified request asynchronously with the given dependency.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="dependency">The dependency required to handle the request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the operation result.
    /// </returns>
    Task<ExecutionResult> HandleAsync(
        TRequest request,
        TDependency dependency,
        CancellationToken cancellationToken = default);
}