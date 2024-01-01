﻿
/************************************************************************************************************
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
************************************************************************************************************/
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.IntegrationEvents;
using Xpandables.Net.SnapShots;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register services.
/// </summary>
public static class ServiceCollectionAggregateExtensions
{
    /// <summary>
    /// Registers the <see cref="DomainDataContext"/> type class to the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="optionsAction">An optional action to configure the 
    /// Microsoft.EntityFrameworkCore.DbContextOptions for the context.</param>
    /// <param name="contextLifetime">The lifetime with which to register the 
    /// context service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register the 
    /// DbContextOptions service in the container.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDomainDataContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));

        services.AddDbContext<DomainDataContext>(optionsAction, contextLifetime, optionsLifetime);
        return services;
    }

    /// <summary>
    /// Registers the default implementation as <see cref="IDomainEventStore"/> to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXDomainEventStore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddXDomainEventStore<DomainEventStore>();
    }

    /// <summary>
    /// Registers the default implementation as <see cref="IIntegrationEventStore"/> to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXIntegrationEventStore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddXIntegrationEventStore<IntegrationEventStore>();
    }

    /// <summary>
    /// Adds the default <see cref="ISnapShotStore"/> snapshot store behavior 
    /// to command handlers with scoped life time.
    /// </summary>
    /// <remarks>You need to define the <see cref="SnapShotOptions"/> in configuration file.</remarks>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddXSnapshotStore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddXSnapshotStore<SnapShotStore>();

        return services;
    }

}