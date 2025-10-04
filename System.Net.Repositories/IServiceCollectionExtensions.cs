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
using System.Net.DependencyInjection;
using System.Net.Repositories;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension methods for registering Entity Framework repository services.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the specified implementation of the unit of work pattern to the service collection with a scoped
        /// lifetime.
        /// </summary>
        /// <remarks>This method registers TUnitOfWork as the implementation for IUnitOfWork using scoped
        /// lifetime. Each scope (such as a web request) will receive its own instance of TUnitOfWork.</remarks>
        /// <typeparam name="TUnitOfWork">The type that implements the IUnitOfWork interface to be registered. Must be a class with a public
        /// constructor.</typeparam>
        /// <returns>The IServiceCollection instance with the unit of work service registered. This enables method chaining.</returns>
        public IServiceCollection AddXUnitOfWork<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork =>
            services.AddScoped<IUnitOfWork, TUnitOfWork>();

        /// <summary>
        /// Adds a keyed scoped registration of the specified unit of work implementation to the service collection.
        /// </summary>
        /// <remarks>Use this method to register multiple IUnitOfWork implementations distinguished by a
        /// key. This enables resolving different unit of work types by key at runtime.</remarks>
        /// <typeparam name="TUnitOfWork">The type of the unit of work to register. Must implement the IUnitOfWork interface and have a public
        /// constructor.</typeparam>
        /// <param name="key">The unique key that identifies the unit of work registration. Cannot be null.</param>
        /// <returns>The IServiceCollection instance with the new keyed unit of work registration added.</returns>
        public IServiceCollection AddXUnitOfWorkKeyed<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TUnitOfWork>(string key)
            where TUnitOfWork : class, IUnitOfWork
        {
            ArgumentNullException.ThrowIfNull(key);
            return services.AddKeyedScoped<IUnitOfWork, TUnitOfWork>(key);
        }

        /// <summary>
        /// Registers the specified unit of work implementation with the dependency injection container using a scoped
        /// lifetime.
        /// </summary>
        /// <remarks>Use this method to configure dependency injection for unit of work patterns in test
        /// scenarios or applications that require scoped lifetimes. Each scope will receive its own instance of the
        /// unit of work implementation.</remarks>
        /// <typeparam name="TInterface">The interface type that represents the unit of work contract. Must implement IUnitOfWork.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that implements the unit of work interface. Must have a public constructor.</typeparam>
        /// <returns>The IServiceCollection instance for chaining additional service registrations.</returns>
        public IServiceCollection AddXUnitOfWork<TInterface, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
            where TInterface : class, IUnitOfWork
            where TImplementation : class, TInterface =>
            services.AddScoped<TInterface, TImplementation>();

        /// <summary>
        /// Registers a keyed scoped unit of work service with the specified interface and implementation types in the
        /// dependency injection container.
        /// </summary>
        /// <remarks>Use this method to register multiple unit of work implementations under different
        /// keys, allowing keyed resolution of IUnitOfWork services. This is useful when your application requires more
        /// than one unit of work implementation to be resolved by key.</remarks>
        /// <typeparam name="TInterface">The interface type of the unit of work to register. Must implement IUnitOfWork.</typeparam>
        /// <typeparam name="TImplementation">The concrete implementation type of the unit of work to register. Must implement TInterface and have a
        /// public constructor.</typeparam>
        /// <param name="key">The unique key that identifies the registration. Cannot be null.</param>
        /// <returns>The IServiceCollection instance for chaining additional service registrations.</returns>
        public IServiceCollection AddXUnitOfWorkKeyed<TInterface, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(string key)
            where TInterface : class, IUnitOfWork
            where TImplementation : class, TInterface
        {
            ArgumentNullException.ThrowIfNull(key);
            return services.AddKeyedScoped<TInterface, TImplementation>(key);
        }

        /// <summary>
        /// Adds a custom repository implementation to the service collection.
        /// </summary>
        /// <typeparam name="TRepository">The repository interface type.</typeparam>
        /// <typeparam name="TImplementation">The repository implementation type.</typeparam>
        /// <param name="lifetime">The service lifetime.</param>
        /// <returns>The service collection so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public IServiceCollection AddXRepository<TRepository, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TRepository : class, IRepository
            where TImplementation : class, TRepository
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAdd(new ServiceDescriptor(typeof(TRepository), typeof(TImplementation), lifetime));

            return services;
        }

        /// <summary>
        /// Adds multiple repository implementations to the service collection.
        /// </summary>
        /// <param name="repositoryRegistrations">The repository registrations containing interface and implementation types.</param>
        /// <param name="lifetime">The service lifetime.</param>
        /// <returns>The service collection so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services or repositoryRegistrations is null.</exception>
        public IServiceCollection AddXRepositories(
            ServiceLifetime lifetime,
            params (Type InterfaceType, Type ImplementationType)[] repositoryRegistrations)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(repositoryRegistrations);
            ArgumentOutOfRangeException.ThrowIfZero(repositoryRegistrations.Length, nameof(repositoryRegistrations));

            foreach (var (interfaceType, implementationType) in repositoryRegistrations)
            {
                if (!typeof(IRepository).IsAssignableFrom(interfaceType))
                {
                    throw new ArgumentException($"Interface type {interfaceType.Name} must implement IRepository.", nameof(repositoryRegistrations));
                }

                if (!interfaceType.IsAssignableFrom(implementationType))
                {
                    throw new ArgumentException($"Implementation type {implementationType.Name} must implement {interfaceType.Name}.", nameof(repositoryRegistrations));
                }

                services.TryAdd(new ServiceDescriptor(interfaceType, implementationType, lifetime));
            }

            return services;
        }
    }
}