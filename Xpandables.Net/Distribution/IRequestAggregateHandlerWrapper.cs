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

namespace Xpandables.Net.Distribution;


/// <summary>
/// Represents a wrapper interface that avoids use of C# dynamics 
/// with decider pattern and allows 
/// type inference for <see cref="IRequestAggregateHandler{TRequest, TAggregate}"/>.
/// </summary>
/// <typeparam name="TAggregate">Type of the aggregate.</typeparam>
public interface IRequestAggregateHandlerWrapper<TAggregate>
    where TAggregate : class, IAggregate
{
    /// <summary>
    /// Asynchronously handles the specified request.
    /// </summary>
    /// <param name="request">The request to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="request"/> is null.</exception>
    /// <returns>A task that represents an 
    /// object of <see cref="IOperationResult"/>.</returns>
    Task<IOperationResult> HandleAsync(
        IRequestAggregate<TAggregate> request,
        CancellationToken cancellationToken = default);
}