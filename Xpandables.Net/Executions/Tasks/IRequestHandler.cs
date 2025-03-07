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

using Xpandables.Net.Executions.Deciders;

namespace Xpandables.Net.Executions.Tasks;
/// <summary>
/// Defines a handler for a request of type <typeparamref name="TRequest"/>.
/// </summary>
/// <remarks>This can also be enhanced with some useful decorators.</remarks>
/// <typeparam name="TRequest">The type of the request.</typeparam>
public interface IRequestHandler<in TRequest>
    where TRequest : class, IRequest
{
    /// <summary>
    /// Handles the specified request asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation 
    /// requests.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the operation result.</returns>
    Task<IExecutionResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for a request of type <typeparamref name="TRequest"/> 
/// with a dependency of type <typeparamref name="TDependency"/>.
/// </summary>
/// <remarks>This can also be enhanced with some useful decorators.</remarks>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public interface IRequestHandler<in TRequest, in TDependency> : IRequestHandler<TRequest>
    where TRequest : class, IRequest, IDecider<TDependency>
    where TDependency : class
{
    /// <summary>
    /// Handles the specified request asynchronously with the given dependency.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="dependency">The dependency required to handle the request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the operation result.</returns>
    Task<IExecutionResult> HandleAsync(
        TRequest request,
        TDependency dependency,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    Task<IExecutionResult> IRequestHandler<TRequest>.HandleAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Dependency is null)
            throw new InvalidOperationException("The dependency is not set.");

        return HandleAsync(request, (TDependency)request.Dependency, cancellationToken);
    }
}
