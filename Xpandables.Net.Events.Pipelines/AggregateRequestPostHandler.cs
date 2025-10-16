
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

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net;
using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Events;

/// <summary>
/// Handles post-processing of requests by appending aggregates to the appropriate store.
/// </summary>
/// <remarks>This handler is designed to be used in scenarios where a request results in an aggregate that needs
/// to be appended to a store. It utilizes the <see cref="IAggregateStore{T}"/> service to perform the append
/// operation.</remarks>
/// <typeparam name="TRequest">The type of the request being handled. 
/// Must implement <see cref="IDependencyRequest"/> and <see
/// cref="IRequiresEventStorage"/>.</typeparam>
/// <param name="serviceProvider"></param>
public sealed class AggregateRequestPostHandler<TRequest>(IServiceProvider serviceProvider)
    : IRequestPostHandler<TRequest>
    where TRequest : class, IDependencyRequest, IRequiresEventStorage
{
    /// <inheritdoc/>
    public async Task<ExecutionResult> HandleAsync(
        RequestContext<TRequest> context,
        ExecutionResult response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(response);

        if (response.IsSuccess)
        {
            Type aggregateStoreType = typeof(IAggregateStore<>)
                .MakeGenericType(context.Request.DependencyType);

            IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
                .GetRequiredService(aggregateStoreType);

            object aggregate = context.Request.DependencyInstance.Value;

            await aggregateStore
                .SaveAsync((Aggregate)aggregate, cancellationToken)
                .ConfigureAwait(false);
        }

        return response;
    }
}
