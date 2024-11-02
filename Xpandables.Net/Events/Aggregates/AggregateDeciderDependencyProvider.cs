
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

using Xpandables.Net.Responsibilities;

namespace Xpandables.Net.Events.Aggregates;
internal sealed class AggregateDeciderDependencyProvider(
    IServiceProvider serviceProvider) : IDeciderDependencyProvider
{
    public async Task<object> GetDependencyAsync(
        IDeciderCommand command,
        CancellationToken cancellationToken = default)
    {
        Type aggregateStoreType = typeof(IAggregateStore<>).MakeGenericType(command.Type);

        dynamic aggregateStore = serviceProvider.GetRequiredService(aggregateStoreType);

        IAggregate aggregate = await aggregateStore
            .PeekAsync(command.KeyId, cancellationToken)
            .ConfigureAwait(false);

        return aggregate;
    }
}
