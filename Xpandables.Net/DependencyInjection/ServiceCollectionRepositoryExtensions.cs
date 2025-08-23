
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

using Xpandables.Net.Repositories;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for adding repository services to 
/// the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionRepositoryExtensions
{
    /// <summary>
    /// Adds a scoped service of the type specified in <typeparamref name="TRepository"/> to the <see
    /// cref="IServiceCollection"/> with an <see cref="IRepository"/> service type.
    /// </summary>
    /// <remarks>This method registers the repository as a scoped service, meaning a new instance will be
    /// created for each request within the scope.</remarks>
    /// <typeparam name="TRepository">The type of the repository to add. 
    /// This type must implement <see cref="IRepository"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the repository will be added.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddXRepository<TRepository>(
        this IServiceCollection services)
        where TRepository : class, IRepository =>
        services.AddScoped<IRepository, TRepository>();

    /// <summary>
    /// Adds a keyed scoped service of type <typeparamref name="TRepository"/> to the specified <see
    /// cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method registers the repository as a scoped service, allowing it to be resolved with the
    /// specified key.</remarks>
    /// <typeparam name="TRepository">The type of the repository to add. 
    /// Must implement <see cref="IRepository"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service is added.</param>
    /// <param name="key">The key associated with the repository service.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddXRepositoryKeyed<TRepository>(
        this IServiceCollection services, string key)
        where TRepository : class, IRepository =>
        services.AddKeyedScoped<IRepository, TRepository>(key);

    /// <summary>
    /// Registers a repository service with a scoped lifetime in the dependency injection container.
    /// </summary>
    /// <remarks>This method registers the specified repository implementation with a scoped lifetime, meaning
    /// a new instance is created for each request within the same scope. Ensure that <typeparamref name="TImplementation"/>
    /// implements <typeparamref name="TInterface"/> to avoid runtime errors.</remarks>
    /// <typeparam name="TInterface">The interface type of the repository to register.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of the repository.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the repository is added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with the repository service registered.</returns>
    public static IServiceCollection AddXRepository<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class, IRepository
        where TImplementation : class, TInterface =>
        services.AddScoped<TInterface, TImplementation>();

    /// <summary>
    /// Adds a keyed scoped service of the specified repository interface and implementation type to the service
    /// collection.
    /// </summary>
    /// <remarks>This method registers the specified repository implementation as a scoped service under the
    /// provided key. The key can be used to resolve the service in scenarios where multiple implementations of the same
    /// interface are registered.</remarks>
    /// <typeparam name="TInterface">The type of the repository interface to register.</typeparam>
    /// <typeparam name="TImplementation">The type of the repository implementation to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service will be added.</param>
    /// <param name="key">The key used to identify the registered service.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXRepositoryKeyed<TInterface, TImplementation>(
        this IServiceCollection services, string key)
        where TInterface : class, IRepository
        where TImplementation : class, TInterface =>
        services.AddKeyedScoped<TInterface, TImplementation>(key);

    /// <summary>
    /// Registers the specified unit of work implementation in the service collection with a scoped lifetime.
    /// </summary>
    /// <remarks>This method is typically used to configure dependency injection for unit of work
    /// implementations in applications that follow the Unit of Work design pattern. The registered implementation will
    /// be resolved as <see cref="IUnitOfWork"/> with a scoped lifetime.</remarks>
    /// <typeparam name="TUnitOfWork">The type of the unit of work implementation to register. This type must implement <see cref="IUnitOfWork"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the unit of work service will be added.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, allowing for method chaining.</returns>
    public static IServiceCollection AddXUnitOfWork<TUnitOfWork>(
        this IServiceCollection services)
        where TUnitOfWork : class, IUnitOfWork =>
        services.AddScoped<IUnitOfWork, TUnitOfWork>();

    /// <summary>
    /// Adds a keyed scoped registration for a unit of work implementation to the service collection.
    /// </summary>
    /// <remarks>This method is typically used to register multiple implementations of <see
    /// cref="IUnitOfWork"/> with distinct keys, enabling keyed resolution of the appropriate implementation at
    /// runtime.</remarks>
    /// <typeparam name="TUnitOfWork">The type of the unit of work implementation to register. This type must implement <see cref="IUnitOfWork"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the registration will be added.</param>
    /// <param name="key">The key used to identify the scoped registration. This key must be unique within the scope of the service
    /// collection.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance, allowing for further chaining of service registrations.</returns>
    public static IServiceCollection AddXUnitOfWorkKeyed<TUnitOfWork>(
        this IServiceCollection services, string key)
        where TUnitOfWork : class, IUnitOfWork =>
        services.AddKeyedScoped<IUnitOfWork, TUnitOfWork>(key);

    /// <summary>
    /// Registers the specified unit of work implementation with the dependency injection container.
    /// </summary>
    /// <remarks>This method registers the unit of work implementation as a scoped service, ensuring that a
    /// new instance is created for each request scope. The implementation type must derive from the specified interface
    /// type.</remarks>
    /// <typeparam name="TInterface">The interface type representing the unit of work.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of the unit of work.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the unit of work is added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXUnitOfWork<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class, IUnitOfWork
        where TImplementation : class, TInterface =>
        services.AddScoped<TInterface, TImplementation>();

    /// <summary>
    /// Adds a keyed scoped registration for the specified unit of work implementation.
    /// </summary>
    /// <remarks>This method registers the specified implementation of <typeparamref name="TInterface"/> as a
    /// keyed scoped service. The key can be used to resolve the service in scenarios where multiple implementations of
    /// the same interface are registered.</remarks>
    /// <typeparam name="TInterface">The interface type representing the unit of work.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of the unit of work.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the registration is added.</param>
    /// <param name="key">The key used to identify the scoped registration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXUnitOfWorkKeyed<TInterface, TImplementation>(
        this IServiceCollection services, string key)
        where TInterface : class, IUnitOfWork
        where TImplementation : class, TInterface =>
        services.AddKeyedScoped<TInterface, TImplementation>(key);

    /// <summary>
    /// Registers a specified implementation of <see cref="IUnitOfWorkEvent"/> in the service collection  with a scoped
    /// lifetime.
    /// </summary>
    /// <remarks>This method is typically used to register a custom implementation of <see
    /// cref="IUnitOfWorkEvent"/>  for dependency injection in applications that use the Unit of Work pattern.</remarks>
    /// <typeparam name="TUnitOfWorkEvent">The type of the class that implements <see cref="IUnitOfWorkEvent"/> to be registered.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the <typeparamref name="TUnitOfWorkEvent"/>  implementation will
    /// be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance, allowing for further chaining of service registrations.</returns>
    public static IServiceCollection AddXUnitOfWorkEvent<TUnitOfWorkEvent>(
        this IServiceCollection services)
        where TUnitOfWorkEvent : class, IUnitOfWorkEvent =>
        services.AddScoped<IUnitOfWorkEvent, TUnitOfWorkEvent>();

    /// <summary>
    /// Adds a keyed scoped registration for a unit of work event implementation to the service collection.
    /// </summary>
    /// <remarks>This method registers the specified <typeparamref name="TUnitOfWorkEvent"/> implementation of
    /// <see cref="IUnitOfWorkEvent"/> as a keyed scoped service. The key can be used to resolve the  appropriate
    /// implementation in scenarios where multiple implementations are registered.</remarks>
    /// <typeparam name="TUnitOfWorkEvent">The type of the unit of work event to register. This type must implement <see cref="IUnitOfWorkEvent"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the registration will be added.</param>
    /// <param name="key">The key used to identify the scoped registration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXUnitOfWorkEventKeyed<TUnitOfWorkEvent>(
        this IServiceCollection services, string key)
        where TUnitOfWorkEvent : class, IUnitOfWorkEvent =>
        services.AddKeyedScoped<IUnitOfWorkEvent, TUnitOfWorkEvent>(key);

    /// <summary>
    /// Registers the specified unit of work event interface and its implementation in the service collection with a
    /// scoped lifetime.
    /// </summary>
    /// <remarks>This method is typically used to register unit of work event handlers in a dependency
    /// injection container. The implementation type <typeparamref name="TImplementation"/> must implement the interface
    /// <typeparamref name="TInterface"/>.</remarks>
    /// <typeparam name="TInterface">The interface type that represents the unit of work event.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of the unit of work event interface.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the unit of work event is added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXUnitOfWorkEvent<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class, IUnitOfWorkEvent
        where TImplementation : class, TInterface =>
        services.AddScoped<TInterface, TInterface>();

    /// <summary>
    /// Adds a keyed scoped registration for the specified unit of work event interface and implementation.
    /// </summary>
    /// <remarks>This method registers the specified implementation type as a scoped service for the given
    /// interface type, using the provided key to distinguish the registration. This is useful for scenarios where
    /// multiple implementations of the same interface need to be resolved by a unique key.</remarks>
    /// <typeparam name="TInterface">The interface type that represents the unit of work event. Must implement <see cref="IUnitOfWorkEvent"/>.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of <typeparamref name="TInterface"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the registration will be added.</param>
    /// <param name="key">The key used to identify the scoped registration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXUnitOfWorkEventKeyed<TInterface, TImplementation>(
        this IServiceCollection services, string key)
        where TInterface : class, IUnitOfWorkEvent
        where TImplementation : class, TInterface =>
        services.AddKeyedScoped<TInterface, TImplementation>(key);

    /// <summary>
    /// Adds the specified implementation of <see cref="IEventStore"/> to the service collection with a scoped lifetime.
    /// </summary>
    /// <remarks>This method registers the specified <typeparamref name="TEventStore"/> as the implementation
    /// of  <see cref="IEventStore"/> with a scoped lifetime. This ensures that a new instance of  <typeparamref
    /// name="TEventStore"/> is created for each request scope.</remarks>
    /// <typeparam name="TEventStore">The type of the class that implements <see cref="IEventStore"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the <see cref="IEventStore"/> implementation is added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStore<TEventStore>(
        this IServiceCollection services)
        where TEventStore : class, IEventStore =>
        services.AddScoped<IEventStore, TEventStore>();

    /// <summary>
    /// Adds a keyed registration for an <see cref="IEventStore"/> implementation to the service collection.
    /// </summary>
    /// <remarks>This method registers the specified <typeparamref name="TEventStore"/> implementation as a
    /// keyed scoped service. The key can be used to resolve the appropriate implementation of <see cref="IEventStore"/>
    /// at runtime.</remarks>
    /// <typeparam name="TEventStore">The type of the <see cref="IEventStore"/> implementation to register. Must be a class that implements <see
    /// cref="IEventStore"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the registration will be added.</param>
    /// <param name="key">The key used to identify the <typeparamref name="TEventStore"/> implementation in the service collection.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStoreKeyed<TEventStore>(
        this IServiceCollection services, string key)
        where TEventStore : class, IEventStore =>
        services.AddKeyedScoped<IEventStore, TEventStore>(key);

    /// <summary>
    /// Adds the specified event store implementation to the service collection with a scoped lifetime.
    /// </summary>
    /// <remarks>This method registers the <typeparamref name="TImplementation"/> as the implementation for
    /// the <typeparamref name="TInterface"/> service with a scoped lifetime. Scoped services are created once per
    /// request within the scope of an HTTP request in ASP.NET Core.</remarks>
    /// <typeparam name="TInterface">The interface type representing the event store.</typeparam>
    /// <typeparam name="TImplementation">The concrete implementation type of the event store.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the event store will be added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStore<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class, IEventStore
        where TImplementation : class, TInterface =>
        services.AddScoped<TInterface, TImplementation>();

    /// <summary>
    /// Adds a keyed scoped service for the specified event store interface and implementation.
    /// </summary>
    /// <remarks>This method registers the specified implementation of the event store interface as a keyed
    /// scoped service. The key allows multiple implementations of the same interface to be registered and resolved
    /// independently.</remarks>
    /// <typeparam name="TInterface">The interface type of the event store. Must implement <see cref="IEventStore"/>.</typeparam>
    /// <typeparam name="TImplementation">The implementation type of the event store. Must derive from <typeparamref name="TInterface"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the service will be added.</param>
    /// <param name="key">The key used to register the service. This key is used to resolve the service instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddXEventStoreKeyed<TInterface, TImplementation>(
        this IServiceCollection services, string key)
        where TInterface : class, IEventStore
        where TImplementation : class, TInterface =>
        services.AddKeyedScoped<TInterface, TImplementation>(key);
}

