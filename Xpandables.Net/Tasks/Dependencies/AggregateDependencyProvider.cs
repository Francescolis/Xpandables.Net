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
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net;
using Xpandables.Net.Cqrs;
using Xpandables.Net.Events;

namespace Xpandables.Net.Tasks.Dependencies;

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
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "The DependencyType is guaranteed to be an IAggregate implementation which is preserved by the dependency injection container.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "The DependencyType is guaranteed to be an IAggregate implementation which is preserved by the dependency injection container.")]
    public async Task<object> GetDependencyAsync(
        IDependencyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.DependencyKeyId is not Guid streamId || streamId == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"The dependency key ID must be a non-empty Guid for aggregate type '{request.DependencyType.Name}'.");
        }

        Type aggregateStoreType = typeof(IAggregateStore<>)
            .MakeGenericType(request.DependencyType);

        IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
            .GetRequiredService(aggregateStoreType);

        // LoadAsync will throw ValidationException if aggregate is not found
        // This is intentional - the caller should handle aggregate not found scenarios
        IAggregate aggregate = await aggregateStore
            .LoadAsync(streamId, cancellationToken)
            .ConfigureAwait(false);

        return aggregate;
    }
}
