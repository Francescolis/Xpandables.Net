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

using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Executions.Deciders;

internal sealed class AggregateDeciderDependencyProvider(
    IServiceProvider serviceProvider) : IDeciderDependencyProvider
{
    public bool CanProvideDependency(Type dependencyType) =>
        typeof(IAggregate).IsAssignableFrom(dependencyType);

    public async Task<object> GetDependencyAsync(
        IDeciderRequest decider, CancellationToken cancellationToken = default)
    {
        try
        {
            Type aggregateStoreType = typeof(IAggregateStore<>)
                .MakeGenericType(decider.Type);

            IAggregateStore aggregateStore = (IAggregateStore)serviceProvider
                .GetRequiredService(aggregateStoreType);

            IAggregate dependency = await aggregateStore
                .PeekAsync((Guid)decider.KeyId, cancellationToken)
                .ConfigureAwait(false);

            return dependency;
        }
        catch (Exception exception)
            when (exception is not ValidationException
                and not InvalidOperationException
                and not UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"An error occurred when getting aggregate type " +
                $"with the key '{decider.KeyId}'.", exception);
        }
    }
}
