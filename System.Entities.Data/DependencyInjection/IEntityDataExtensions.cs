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
using System.Entities;
using System.Entities.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension methods for registering Entity Framework repository services.
/// </summary>
public static class IEntityDataExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the specified data context type to the service collection with configurable options and lifetimes.
        /// </summary>
        /// <remarks>Use this method to register a custom data context for dependency injection, allowing
        /// configuration of its options and service lifetimes. This is typically called during application
        /// startup.</remarks>
        /// <typeparam name="TDataContext">The type of the data context to register. Must inherit from DataContext.</typeparam>
        /// <param name="optionsAction">An optional action to configure the DbContext options for the data context. If null, default options are
        /// used.</param>
        /// <param name="contextLifetime">The lifetime with which to register the data context. The default is Scoped.</param>
        /// <param name="optionsLifetime">The lifetime with which to register the options instance. The default is Scoped.</param>
        /// <returns>The same IServiceCollection instance so that additional calls can be chained.</returns>
        public IServiceCollection AddXDataContext<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TDataContext>(
            Action<DbContextOptionsBuilder>? optionsAction = null,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TDataContext : DataContext =>
            services.AddDbContext<TDataContext>(
                optionsAction, contextLifetime, optionsLifetime);

        /// <summary>
        /// Adds a factory for creating instances of the specified data context type to the service collection.
        /// </summary>
        /// <remarks>Use this method to enable dependency injection of factories for <typeparamref
        /// name="TDataContext"/>. The factory can be used to create data context instances with the configured options.
        /// This is useful for scenarios where data contexts need to be created on demand, such as in background
        /// services or for multi-tenancy.</remarks>
        /// <typeparam name="TDataContext">The type of data context to create. Must inherit from <see cref="DataContext"/>.</typeparam>
        /// <param name="optionsAction">A delegate that configures the <see cref="DbContextOptionsBuilder"/> for the data context. Receives the
        /// current <see cref="IServiceProvider"/> and the options builder.</param>
        /// <param name="factoryLifetime">The lifetime with which to register the data context factory. Defaults to <see
        /// cref="ServiceLifetime.Singleton"/>.</param>
        /// <returns>The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
        public IServiceCollection AddXDataContextFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TDataContext>(
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime factoryLifetime = ServiceLifetime.Singleton)
            where TDataContext : DataContext
            => services.AddDbContextFactory<TDataContext>(
                optionsAction, factoryLifetime);

        /// <summary>
        /// Adds default unit of work services to the specified service collection.
        /// </summary>
        /// <returns>The service collection so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public IServiceCollection AddXUnitOfWork()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        /// <summary>
        /// Adds default unit of work services with a specific <see cref="DataContext"/>> to the specified service collection.
        /// </summary>
        /// <typeparam name="TDataContext">The type of the DataContext.</typeparam>
        /// <returns>The service collection so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public IServiceCollection AddXUnitOfWork<TDataContext>()
            where TDataContext : DataContext
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddXUnitOfWork<IUnitOfWork<TDataContext>, UnitOfWork<TDataContext>>();
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<IUnitOfWork<TDataContext>>());

            return services;
        }

        /// <summary>
        /// Adds the default repository implementation to the service collection.
        /// </summary>
        /// <param name="lifetime">The service lifetime.</param>
        /// <returns>The service collection so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public IServiceCollection AddXRepository(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAdd(new ServiceDescriptor(typeof(IRepository<>), typeof(Repository<>), lifetime));

            return services;
        }
    }
}