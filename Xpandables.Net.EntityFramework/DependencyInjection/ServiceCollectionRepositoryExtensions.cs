
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding services related to DataContextEvent.
/// </summary>
public static class ServiceCollectionRepositoryExtensions
{
    /// <summary>
    /// Adds a specified DataContext to the service collection with the specified options.
    /// </summary>
    /// <typeparam name="TDataContext">The type of the data context to add.</typeparam>
    /// <param name="services">The service collection to add the context to.</param>
    /// <param name="optionsAction">An optional action to configure the 
    /// <see cref="DbContextOptionsBuilder"/>.</param>
    /// <param name="contextLifetime">The lifetime with which to register 
    /// the context service in the container.</param>
    /// <param name="optionsLifetime">The lifetime with which to register 
    /// the options service in the container.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddXDataContext<TDataContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDataContext : DataContext =>
        services.AddDbContext<TDataContext>(
            optionsAction, contextLifetime, optionsLifetime);

    /// <summary>
    /// Adds the DataContextEvent to the service collection with the specified 
    /// options.
    /// </summary>
    /// <param name="services">The service collection to add the context to.</param>
    /// <param name="optionAction">An action to configure the 
    /// <see cref="DbContextOptionsBuilder"/>.</param>
    /// <returns>The same service collection so that multiple calls can be 
    /// chained.</returns>
    public static IServiceCollection AddXDataContextEvent(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionAction) =>
        services.AddXDataContext<DataContextEvent>(optionAction);

    /// <summary>
    /// Registers the repository service for the specified data context type in the dependency injection container.
    /// </summary>
    /// <remarks>This method registers the repository as a scoped service, meaning a new instance will be
    /// created for each request.</remarks>
    /// <typeparam name="TDataContext">The type of the data context to be used by the repository. Must derive from <see cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the repository service will be added.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance, allowing for method chaining.</returns>
    public static IServiceCollection AddXRepository<TDataContext>(
        this IServiceCollection services)
        where TDataContext : DataContext =>
        services.AddXRepository<Repository<TDataContext>>();

    /// <summary>
    /// Registers a keyed repository of type <see cref="Repository{TDataContext}"/> in the service collection.
    /// </summary>
    /// <remarks>This method is a convenience overload that registers a repository of type <see
    /// cref="Repository{TDataContext}"/>  with the specified key. The repository can later be resolved using the same
    /// key.</remarks>
    /// <typeparam name="TDataContext">The type of the data context used by the repository. Must derive from <see cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the repository will be added.</param>
    /// <param name="key">A unique string key used to identify the repository instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXRepositoryKeyed<TDataContext>(
        this IServiceCollection services, string key)
        where TDataContext : DataContext =>
        services.AddXRepositoryKeyed<Repository<TDataContext>>(key);

    /// <summary>
    /// Registers the unit of work implementation for the specified data context type in the service collection.
    /// </summary>
    /// <remarks>This method adds a scoped registration of <see cref="IUnitOfWork{TDataContext}"/> with its
    /// implementation <see cref="UnitOfWork{TDataContext}"/> for the specified data context type.</remarks>
    /// <typeparam name="TDataContext">The type of the data context to be used with the unit of work. This type must derive from <see
    /// cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the unit of work service will be added.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, allowing for method chaining.</returns>
    public static IServiceCollection AddXUnitOfWork<TDataContext>(
        this IServiceCollection services)
        where TDataContext : DataContext =>
        services.AddXUnitOfWork<UnitOfWork<TDataContext>>();

    /// <summary>
    /// Adds a keyed unit of work for the specified data context type to the service collection.
    /// </summary>
    /// <remarks>This method registers a unit of work implementation keyed by the specified <paramref
    /// name="key"/>.  It is useful in scenarios where multiple units of work need to be distinguished by a unique
    /// identifier.</remarks>
    /// <typeparam name="TDataContext">The type of the data context to be used with the unit of work. This type must derive from <see
    /// cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the keyed unit of work will be added.</param>
    /// <param name="key">The unique key associated with the unit of work. This key is used to differentiate between multiple units of
    /// work.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance, enabling method chaining.</returns>
    public static IServiceCollection AddXUnitOfWorkKeyed<TDataContext>(
        this IServiceCollection services, string key)
        where TDataContext : DataContext =>
        services.AddXUnitOfWorkKeyed<UnitOfWork<TDataContext>>(key);

    /// <summary>
    /// Adds the default implementation of the unit of work event handling mechanism to the service collection.
    /// </summary>
    /// <remarks>This method registers the default <see cref="UnitOfWorkEvent{TContext}"/> implementation for
    /// handling unit of work events with the specified data context type.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the unit of work event handling is added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXUnitOfWorkEvent(
        this IServiceCollection services) =>
        services.AddXUnitOfWorkEvent<UnitOfWorkEvent<DataContextEvent>>();

    /// <summary>
    /// Adds the default implementation of the unit of work event for the specified data context type to the service
    /// collection.
    /// </summary>
    /// <remarks>This method is an extension method for <see cref="IServiceCollection"/> and is used to
    /// register a unit of work event implementation for a specific data context type. It simplifies the configuration
    /// of dependency injection for unit of work patterns.</remarks>
    /// <typeparam name="TDataContext">The type of the data context for which the unit of work event is being registered.  This type must derive from
    /// <see cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the unit of work event will be added.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance with the unit of work event registered.</returns>
    public static IServiceCollection AddXUnitOfWorkEvent<TDataContext>(
        this IServiceCollection services)
        where TDataContext : DataContext =>
        services.AddXUnitOfWorkEvent<UnitOfWorkEvent<TDataContext>>();

    /// <summary>
    /// Registers a keyed unit of work event for the specified data context type in the service collection.
    /// </summary>
    /// <remarks>This method is a convenience wrapper for registering a keyed <see
    /// cref="UnitOfWorkEvent{TDataContext}"/> in the service collection. The key is used to differentiate between
    /// multiple unit of work events that may be registered for the same data context type.</remarks>
    /// <typeparam name="TDataContext">The type of the data context associated with the unit of work event. This type must derive from <see
    /// cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the unit of work event will be added.</param>
    /// <param name="key">The unique key used to identify the unit of work event within the service collection.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance, allowing for further chaining of service registrations.</returns>
    public static IServiceCollection AddXUnitOfWorkEventKeyed<TDataContext>(
        this IServiceCollection services, string key)
        where TDataContext : DataContext =>
        services.AddXUnitOfWorkEventKeyed<UnitOfWorkEvent<TDataContext>>(key);

    /// <summary>
    /// Adds the default implementation of the XEventStore to the specified service collection.
    /// </summary>
    /// <remarks>This method registers the default implementation of <see cref="EventStore{TEvent}"/> with the
    /// dependency injection container. It is a convenience method for adding the EventStore with a predefined event
    /// type.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the EventStore will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStore(
        this IServiceCollection services) =>
        services.AddXEventStore<EventStore<DataContextEvent>>();

    /// <summary>
    /// Adds the EventStore service to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method registers the EventStore service with the dependency injection container,  using
    /// the specified data context type. The data context type must inherit from <see cref="DataContext"/>.</remarks>
    /// <typeparam name="TDataContext">The type of the data context used by the event store. Must derive from <see cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the EventStore service will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStore<TDataContext>(
        this IServiceCollection services)
        where TDataContext : DataContext =>
        services.AddXEventStore<EventStore<TDataContext>>();

    /// <summary>
    /// Adds a keyed <see cref="EventStore{TDataContext}"/> to the service collection.
    /// </summary>
    /// <remarks>This method registers an <see cref="EventStore{TDataContext}"/> instance with the specified
    /// key. The key is used to differentiate between multiple event store instances in the service
    /// collection.</remarks>
    /// <typeparam name="TDataContext">The type of the data context used by the event store. Must derive from <see cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the keyed event store will be added.</param>
    /// <param name="key">The unique key used to register the event store instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStoreKeyed<TDataContext>(
        this IServiceCollection services, string key)
        where TDataContext : DataContext =>
        services.AddXEventStoreKeyed<EventStore<TDataContext>>(key);

    /// <summary>
    /// Registers the default implementation of <see cref="IOutboxStore"/> using the specified  <typeparamref
    /// name="TDataContext"/> as the data context.
    /// </summary>
    /// <remarks>This method registers the <see cref="OutboxStore{TDataContext}"/> as a scoped service  for
    /// the <see cref="IOutboxStore"/> interface. Ensure that <typeparamref name="TDataContext"/>  is properly
    /// configured in the application's dependency injection container.</remarks>
    /// <typeparam name="TDataContext">The type of the data context to be used by the <see cref="OutboxStore{TDataContext}"/>.  Must derive from <see
    /// cref="DataContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the <see cref="IOutboxStore"/> service is added.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance with the <see cref="IOutboxStore"/> service registered.</returns>
    public static IServiceCollection AddXOutboxStore<TDataContext>(
        this IServiceCollection services)
        where TDataContext : DataContext
    {
        services.TryAddScoped<IOutboxStore, OutboxStore<TDataContext>>();
        return services;
    }

    /// <summary>
    /// Adds the default implementation of <see cref="IOutboxStore"/> to the service collection.
    /// </summary>
    /// <remarks>This method registers the <see cref="OutboxStore{T}"/> implementation of <see
    /// cref="IOutboxStore"/>  with a scoped lifetime. It is intended to be used in applications that require outbox
    /// pattern support.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the outbox store will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXOutboxStore(
        this IServiceCollection services)
    {
        services.TryAddScoped<IOutboxStore, OutboxStore<DataContextEvent>>();
        return services;
    }
}
