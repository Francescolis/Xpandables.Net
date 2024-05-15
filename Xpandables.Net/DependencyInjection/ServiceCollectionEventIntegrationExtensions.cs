
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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Aggregates;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with a set of static methods to register integration event 
/// services in the dependency injection container.
/// </summary>
public static class ServiceCollectionEventIntegrationExtensions
{
    internal readonly static MethodInfo AddEventIntegrationHandlerMethod
        = typeof(ServiceCollectionEventIntegrationExtensions)
        .GetMethod(nameof(AddXEventIntegrationHandler))!;

    /// <summary>
    /// Adds the <typeparamref name="TEventIntegrationHandler"/> to the services 
    /// with scope life time using the factory if specified.
    /// </summary>
    /// <typeparam name="TEventIntegration">The type of the integration event
    /// .</typeparam>
    /// <typeparam name="TEventIntegrationHandler">The type of the integration 
    /// event handler.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="implementationHandlerFactory">The factory that creates 
    /// the integration event handler.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventIntegrationHandler
        <TEventIntegration, TEventIntegrationHandler>(
        this IServiceCollection services,
        Func<IServiceProvider, TEventIntegrationHandler>?
        implementationHandlerFactory = default)
        where TEventIntegrationHandler : class,
        IEventIntegrationHandler<TEventIntegration>
        where TEventIntegration : notnull, IEventIntegration
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.DoRegisterTypeServiceLifeTime
            <IEventIntegrationHandler<TEventIntegration>, TEventIntegrationHandler>(
            implementationHandlerFactory);
    }

    /// <summary>
    /// Adds the <see cref="IEventIntegrationHandler{TNotification}"/> 
    /// implementations to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="assemblies">The assemblies to scan for implemented types. 
    /// If not set, the calling assembly will be used.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="assemblies"/> is null.</exception>
    public static IServiceCollection AddXEventIntegrationHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];

        return services.DoRegisterInterfaceWithMethodFromAssemblies(
            typeof(IEventIntegrationHandler<>),
            AddEventIntegrationHandlerMethod,
            assemblies);
    }

    /// <summary>
    /// Registers the implementation as <see cref="IEventIntegrationStore"/> to 
    /// the services with scope life time.
    /// </summary>
    /// <typeparam name="TEventIntegrationStore">The type of that implements 
    /// <see cref="IEventIntegrationStore"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventIntegrationStore
        <TEventIntegrationStore>(this IServiceCollection services)
        where TEventIntegrationStore : class, IEventIntegrationStore
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventIntegrationStore),
                typeof(TEventIntegrationStore),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the default implementation as <see cref="IEventIntegrationStore"/>
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    public static IServiceCollection AddXEventIntegrationStore(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddXEventIntegrationStore
            <EventIntegrationStore<EventEntityIntegration>>();
    }

    /// <summary>
    /// Registers the default <see cref="IEventIntegrationPublisher"/> 
    /// implementation to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventIntegrationPublisher(
        this IServiceCollection services)
        => services.AddXEventIntegrationPublisher<EventIntegrationPublisher>();

    /// <summary>
    /// Registers the <typeparamref name="TEventIntegrationPublisher"/> 
    /// as <see cref="IEventIntegrationPublisher"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="TEventIntegrationPublisher">The integration event publisher 
    /// type implementation.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXEventIntegrationPublisher
        <TEventIntegrationPublisher>(
        this IServiceCollection services)
        where TEventIntegrationPublisher : class, IEventIntegrationPublisher
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAdd(
            new ServiceDescriptor(
                typeof(IEventIntegrationPublisher),
                typeof(TEventIntegrationPublisher),
                ServiceLifetime.Scoped));

        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TTransientPublisher"/> type as 
    /// <see cref="ITransientPublisher"/> to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TTransientPublisher">The type that implements 
    /// <see cref="ITransientPublisher"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXTransientPublisher
        <TTransientPublisher>(this IServiceCollection services)
        where TTransientPublisher : class, ITransientPublisher
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITransientPublisher, TTransientPublisher>();
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="TTransientSubscriber"/> type as 
    /// <see cref="ITransientSubscriber"/> 
    /// to the services with scoped life time.
    /// </summary>
    /// <typeparam name="TTransientSubscriber">The type that implements 
    /// <see cref="ITransientSubscriber"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXTransientSubscriber
        <TTransientSubscriber>(this IServiceCollection services)
        where TTransientSubscriber : class, ITransientSubscriber
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ITransientSubscriber, TTransientSubscriber>();
        return services;
    }

    /// <summary>
    /// Registers the default <see cref="ITransientSubscriber"/> implementation 
    /// to the services with scoped life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IServiceCollection AddXTransientSubscriber(
        this IServiceCollection services)
        => services.AddXTransientPublisher<TransientPublisherSubscriber>();
}
