
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
using Xpandables.Net.Aggregates;
using Xpandables.Net.Operations;

namespace Xpandables.Net;

/// <summary>
/// Represents a method signature to be used to apply 
/// <see cref="IRequestAggregateHandler{TRequest, TAggregate}"/> implementation.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TRequest">Type of the request to act on.</typeparam>
/// <param name="request">The request instance to act on.</param>
/// <param name="cancellationToken">A CancellationToken to 
/// observe while waiting for the task to complete.</param>
/// <returns>A value that represents an <see cref="IOperationResult"/>.</returns>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="request"/> is null.</exception>
public delegate Task<IOperationResult> RequestAggregateHandler
    <TRequest, TAggregate>(
    TRequest request, CancellationToken cancellationToken = default)
    where TAggregate : class, IAggregate
    where TRequest : class, IRequestAggregate<TAggregate>;

/// <summary>
/// Provides with a method to handle requests that are associated with an 
/// aggregate using the Decider pattern.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TRequest">The type of the aggregate request.</typeparam>
public interface IRequestAggregateHandler<TRequest, TAggregate>
    where TAggregate : class, IAggregate
    where TRequest : class, IRequestAggregate<TAggregate>
{
    /// <summary>
    /// Handles the specified request for the specified aggregate.
    /// </summary>
    /// <remarks>The target aggregate will be supplied by the aspect.</remarks>
    /// <param name="request">The request instance.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    Task<IOperationResult> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
}
