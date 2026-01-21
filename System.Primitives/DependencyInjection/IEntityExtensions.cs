/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Entities;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension methods for registering Entity repository services.
/// </summary>
public static class IEntityExtensions
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
        /// Adds multiple repositories that implement <see cref="IRepository"/> interface to the service collection.
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

        /// <summary>
        /// Registers repository implementations found in the specified assemblies with the provided service lifetime.
        /// </summary>
        /// <remarks>This method scans the given assemblies for sealed, non-abstract classes that
        /// implement IRepository and registers them with the dependency injection container using their implemented
        /// interfaces. Only interfaces derived from IRepository are registered. Ensure that the assemblies contain the
        /// desired repository implementations before calling this method.</remarks>
        /// <param name="lifetime">The lifetime to use when registering repository services. Determines how long each service instance is
        /// retained by the dependency injection container.</param>
        /// <param name="assemblies">An array of assemblies to scan for repository implementations. If no assemblies are specified, the calling
        /// assembly is used by default.</param>
        /// <returns>The IServiceCollection instance that can be used to further configure services.</returns>
        [RequiresUnreferencedCode("Requires unreferenced code.")]
        public IServiceCollection AddXRepositories(
            ServiceLifetime lifetime,
            params Assembly[] assemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(assemblies);

            assemblies = assemblies.Length == 0 ? [Assembly.GetCallingAssembly()] : assemblies;

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                var repositoryTypes = types
                    .Where(t => t.IsClass && t.IsSealed && !t.IsAbstract && typeof(IRepository).IsAssignableFrom(t));

                foreach (var implementationType in repositoryTypes)
                {
                    var interfaceTypes = implementationType.GetInterfaces()
                        .Where(i => i != typeof(IRepository) && typeof(IRepository).IsAssignableFrom(i));
                    foreach (var interfaceType in interfaceTypes)
                    {
                        services.TryAdd(new ServiceDescriptor(interfaceType, implementationType, lifetime));
                    }
                }
            }

            return services;
        }
    }
}