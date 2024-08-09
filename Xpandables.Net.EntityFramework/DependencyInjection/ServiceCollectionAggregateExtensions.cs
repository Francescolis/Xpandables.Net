
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Aggregates;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register services.
/// </summary>
public static class ServiceCollectionAggregateExtensions
{
    /// <summary>
    /// Registers the <see cref="DataContextEvent"/> type class to the 
    /// services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="optionsAction">An optional action to configure the 
    /// Microsoft.EntityFrameworkCore.DbContextOptions for the context.</param>
    /// <param name="contextLifetime">The lifetime with which to register the 
    /// context service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register the 
    /// DbContextOptions service in the container.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXDataContextEvent(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));

        return services.AddDbContext<DataContextEvent>(
            optionsAction, contextLifetime, optionsLifetime);
    }
}
