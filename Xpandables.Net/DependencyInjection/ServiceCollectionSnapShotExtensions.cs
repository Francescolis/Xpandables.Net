
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
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Aggregates;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides a set of static methods for <see cref="IServiceCollection"/> to
/// register <see cref="IEventSnapshotStore"/>
/// </summary>
public static class ServiceCollectionSnapshotExtensions
{
    /// <summary>
    /// Adds the specified type as <see cref="IEventSnapshotStore"/> snapshot 
    /// store behavior to command handlers with scoped life time.
    /// </summary>
    /// <remarks>You need to define the <see cref="SnapshotOptions"/> in 
    /// configuration file.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventSnapshotStore<TSnapshotStore>(
        this IServiceCollection services)
        where TSnapshotStore : class, IEventSnapshotStore
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped<IEventSnapshotStore, TSnapshotStore>();
    }
}
