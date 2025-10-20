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
using System.ComponentModel;

using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net.Cqrs;

/// <summary>
/// Defines a handler for a request of type <typeparamref name="TRequest" />
/// with a dependency of type <typeparamref name="TDependency" />.
/// </summary>
/// <remarks>This can also be enhanced with some useful decorators.</remarks>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TDependency">The type of the dependency.</typeparam>
public interface IDependencyRequestHandler<in TRequest, in TDependency> : IRequestHandler<TRequest>
    where TRequest : class, IDependencyRequest<TDependency>
    where TDependency : class;

/// <summary>
/// Defines a handler for processing dependency-based requests within a specific context.
/// </summary>
/// <remarks>This interface extends <see cref="IDependencyRequestHandler{TRequest, TDependency}"/> by adding
/// support for handling requests within a contextual wrapper, represented by <see cref="RequestContext{TRequest}"/>. It
/// is designed for scenarios where additional contextual information is required to process the request.</remarks>
/// <typeparam name="TRequest">The type of the request being handled. Must implement <see cref="IDependencyRequest{TDependency}"/>.</typeparam>
/// <typeparam name="TDependency">The type of the dependency associated with the request. Must be a reference type.</typeparam>
public interface IDependencyRequestContextHandler<TRequest, TDependency> : IDependencyRequestHandler<TRequest, TDependency>
    where TRequest : class, IDependencyRequest<TDependency>
    where TDependency : class
{
    /// <summary>
    /// Handles the specified request asynchronously and returns the result of the execution.
    /// </summary>
    /// <param name="context">The request context containing the request and additional contextual information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. T
    /// he default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous operation. 
    /// The task result contains the execution result of type
    /// <see cref="ExecutionResult"/>.</returns>
    Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        CancellationToken cancellationToken = default);

    [EditorBrowsable(EditorBrowsableState.Never)]
    async Task<ExecutionResult> IRequestHandler<TRequest>.HandleAsync(
        TRequest request, CancellationToken cancellationToken) =>
        await HandleAsync(new(request), cancellationToken).ConfigureAwait(false);
}