
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
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides aggregate dependencies by resolving and loading aggregates from the underlying service provider.
/// </summary>
/// <remarks>This provider is intended for use in scenarios where aggregate roots are required as dependencies. It
/// resolves the appropriate aggregate store for the requested aggregate type and loads the aggregate instance using the
/// provided dependency key. This class is thread-safe and can be used concurrently across multiple requests.</remarks>
/// <param name="serviceProvider">The service provider used to resolve aggregate store services required for dependency resolution.</param>
public sealed class AggregateDependencyProvider(IServiceProvider serviceProvider) : IDependencyProvider
{
    /// <inheritdoc />
    public bool CanProvideDependency(Type dependencyType)
    {
        ArgumentNullException.ThrowIfNull(dependencyType);

        return dependencyType.IsAssignableTo(typeof(IAggregate));
    }

    /// <inheritdoc />
    public async Task<object> GetDependencyAsync(
        IDependencyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        Type aggregateStoreType = typeof(IAggregateStore<>)
            .MakeGenericType(request.DependencyType);

        IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
            .GetRequiredService(aggregateStoreType);

        return await aggregateStore
            .LoadAsync((Guid)request.DependencyKeyId, cancellationToken)
            .ConfigureAwait(false);
    }
}
