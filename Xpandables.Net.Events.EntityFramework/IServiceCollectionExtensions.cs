
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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Events;
using Xpandables.Net.Repositories;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering event store and outbox store services with an <see
/// cref="IServiceCollection"/> in applications using the XEvent and Outbox patterns.
/// </summary>
/// <remarks>These extension methods simplify the configuration of event sourcing and outbox services by
/// registering default or custom implementations with the dependency injection container. Use these methods to add
/// support for event storage and outbox processing in your application's service pipeline.</remarks>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="EventStoreDataContext"/>to the service collection with the specified 
    /// options.
    /// </summary>
    /// <param name="services">The service collection to add the context to.</param>
    /// <param name="optionAction">An action to configure the 
    /// <see cref="DbContextOptionsBuilder"/>.</param>
    /// <returns>The same service collection so that multiple calls can be 
    /// chained.</returns>
    [RequiresUnreferencedCode("This method may be removed in a future version.")]
    public static IServiceCollection AddXEventStoreDataContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionAction)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddXDataContext<EventStoreDataContext>(optionAction);
    }

    /// <summary>
    /// Adds the <see cref="OutboxStoreDataContext"/>to the service collection with the specified 
    /// options.
    /// </summary>
    /// <param name="services">The service collection to add the context to.</param>
    /// <param name="optionAction">An action to configure the 
    /// <see cref="DbContextOptionsBuilder"/>.</param>
    /// <returns>The same service collection so that multiple calls can be 
    /// chained.</returns>
    [RequiresUnreferencedCode("This method may be removed in a future version.")]
    public static IServiceCollection AddXOutboxStoreDataContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionAction)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddXDataContext<OutboxStoreDataContext>(optionAction);
    }

    /// <summary>
    /// Adds the default implementation of the EventStore to the specified service collection.
    /// </summary>
    /// <remarks>This method registers the default implementation of <see cref="EventStore"/> with the
    /// dependency injection container. It is a convenience method for adding the EventStore with a predefined event
    /// type.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the EventStore will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddXEventStore<EventStore>();
    }

    /// <summary>
    /// Adds the EventStore service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method registers the EventStore service with the dependency injection container,  using
    /// the specified data context type. The data context type must inherit from <see cref="DataContext"/>.</remarks>
    /// <typeparam name="TEventStore">The type of the data context used by the event store. Must derive from <see cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the EventStore service will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStore<TEventStore>(this IServiceCollection services)
        where TEventStore : class, IEventStore
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddScoped<IEventStore, EventStore>();
        return services;
    }

    /// <summary>
    /// Registers the specified outbox store implementation as a scoped service in the dependency injection container.
    /// </summary>
    /// <remarks>This method adds <typeparamref name="TOutboxStore"/> as the implementation for <see
    /// cref="IOutboxStore"/> with a scoped lifetime. Use this method to configure custom outbox store implementations
    /// for dependency injection.</remarks>
    /// <typeparam name="TOutboxStore">The type of outbox store to register. Must implement <see cref="IOutboxStore"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the outbox store service will be added. Cannot be <see
    /// langword="null"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
    public static IServiceCollection AddXOutboxStore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TOutboxStore>(this IServiceCollection services)
        where TOutboxStore : class, IOutboxStore
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<IOutboxStore, TOutboxStore>();
        return services;
    }

    /// <summary>
    /// Adds the default implementation of <see cref="IOutboxStore"/> to the service collection.
    /// </summary>
    /// <remarks>This method registers the <see cref="OutboxStore"/> implementation of <see
    /// cref="IOutboxStore"/>  with a scoped lifetime. It is intended to be used in applications that require outbox
    /// pattern support.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the outbox store will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXOutboxStore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddXOutboxStore<OutboxStore>();
    }

    /// <summary>
    /// Adds the default XEvent unit of work implementation to the service collection for dependency injection.
    /// </summary>
    /// <remarks>This method registers the EventUnitOfWork as the implementation for XEvent unit of work. Call
    /// this method during application startup to enable XEvent unit of work support in the dependency injection
    /// container.</remarks>
    /// <param name="services">The service collection to which the XEvent unit of work will be registered. Cannot be null.</param>
    /// <returns>The same IServiceCollection instance, enabling method chaining.</returns>
    public static IServiceCollection AddXEventUnitOfWork(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddXEventUnitOfWork<EventUnitOfWork>();
    }

    /// <summary>
    /// Registers the specified event unit of work type with the dependency injection container, associating it with the
    /// event store key.
    /// </summary>
    /// <remarks>Use this method to enable event-driven unit of work patterns in your application by
    /// registering a custom implementation. The registration is keyed to the event store, allowing for resolution of
    /// the unit of work in event processing scenarios.</remarks>
    /// <typeparam name="TUnitOfWork">The unit of work type to register. Must be a class implementing <see cref="IUnitOfWork"/> and have a public
    /// constructor.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the event unit of work will be added. Cannot be <see
    /// langword="null"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, to allow for method chaining.</returns>
    public static IServiceCollection AddXEventUnitOfWork<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TUnitOfWork>(this IServiceCollection services)
        where TUnitOfWork : class, IUnitOfWork
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddXUnitOfWorkKeyed<TUnitOfWork>(nameof(IEventStore));
        return services;
    }
}
