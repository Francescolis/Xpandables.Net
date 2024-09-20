
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

// Ignore Spelling: Impl

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Events;
using Xpandables.Net.HostedServices;
using Xpandables.Net.Internals;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register services.
/// </summary>
public static class ServiceCollectionHostedServiceExtensions
{
    /// <summary>
    /// Adds the <typeparamref name="TBackgroundService"/> type implementation 
    /// to the services with singleton life time.
    /// </summary>
    /// <typeparam name="TBackgroundService">The background service type 
    /// implementation.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXBackgroundService<TBackgroundService>(
        this IServiceCollection services)
        where TBackgroundService : class, IBackgroundService
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddHostedService<TBackgroundService>();
        return services;
    }

    /// <summary>
    /// Adds the <typeparamref name="TBackgroundServiceImpl"/> as 
    /// <typeparamref name="TBackgroundServiceInterface"/>
    /// type implementation to the services with singleton life time.
    /// </summary>
    /// <typeparam name="TBackgroundServiceInterface">The background service 
    /// type interface.</typeparam>
    /// <typeparam name="TBackgroundServiceImpl">The background service type 
    /// implementation.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXBackgroundService
        <TBackgroundServiceInterface, TBackgroundServiceImpl>(
        this IServiceCollection services)
        where TBackgroundServiceInterface : class, IBackgroundService
        where TBackgroundServiceImpl :
        BackgroundServiceBase
        <TBackgroundServiceImpl>, TBackgroundServiceInterface
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddSingleton
            <TBackgroundServiceInterface, TBackgroundServiceImpl>();
        _ = services.AddHostedService(provider =>
        (TBackgroundServiceImpl)provider
            .GetRequiredService<TBackgroundServiceInterface>());
        return services;
    }

    /// <summary>
    /// Adds the specified background service implementation 
    /// event scheduler of <see cref="IEventScheduler"/>
    /// to manage integration event publishing.
    /// </summary>
    /// <typeparam name="TEventIntegrationScheduler">The type that implements 
    /// <see cref="IEventScheduler"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXEventIntegrationScheduler
        <TEventIntegrationScheduler>(this IServiceCollection services)
        where TEventIntegrationScheduler :
        BackgroundServiceBase<TEventIntegrationScheduler>, IEventScheduler
    {
        ArgumentNullException.ThrowIfNull(services);

        _ = services.AddXBackgroundService
            <IEventScheduler, TEventIntegrationScheduler>();

        return services;
    }

    /// <summary>
    /// Adds the default background service implementation integration
    /// event scheduler of <see cref="IEventScheduler"/>
    /// type to manage integration event publishing.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventIntegrationScheduler(
        this IServiceCollection services)
        => services.AddXEventIntegrationScheduler<EventScheduler>();
}
