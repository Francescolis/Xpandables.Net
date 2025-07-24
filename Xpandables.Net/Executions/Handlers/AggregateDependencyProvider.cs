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

using Xpandables.Net.Executions.Dependencies;
using Xpandables.Net.Executions.Domains;
using Xpandables.Net.Executions.Tasks;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Executions.Handlers;

/// <summary>
/// Provides dependencies for aggregate and event sourcing types by utilizing a service provider.
/// </summary>
/// <remarks>This class implements the <see cref="IDependencyProvider"/> interface to supply dependencies
/// specifically for types related to aggregates and event sourcing. It uses a provided <see cref="IServiceProvider"/>
/// to resolve the necessary services.</remarks>
/// <param name="serviceProvider"></param>
public sealed class AggregateDependencyProvider(IServiceProvider serviceProvider) : IDependencyProvider
{
    /// <inheritdoc />
    public bool CanProvideDependency(Type dependencyType) =>
        dependencyType.IsAssignableTo(typeof(Aggregate)) ||
        dependencyType.IsAssignableTo(typeof(IEventSourcing));

    /// <inheritdoc />
    public async Task<object> GetDependencyAsync(
        IDependencyRequest request,
        CancellationToken cancellationToken = default)
    {
        Type aggregateStoreType = typeof(IAggregateStore<>)
            .MakeGenericType(request.DependencyType);

        IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
            .GetRequiredService(aggregateStoreType);

        return await aggregateStore
            .ResolveAsync((Guid)request.DependencyKeyId, cancellationToken)
            .ConfigureAwait(false);
    }
}
