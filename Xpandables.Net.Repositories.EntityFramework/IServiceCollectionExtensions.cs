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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Repositories;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.DependencyInjection;
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
        /// Adds Entity Framework Core repository services to the specified service collection.
        /// </summary>
        /// <returns>The service collection so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public IServiceCollection AddXEntityFrameworkRepositories()
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddScoped<IRepository, EntityFrameworkRepository>();
            services.TryAddScoped<IUnitOfWork, EntityFrameworkUnitOfWork>();

            return services;
        }

        /// <summary>
        /// Adds Entity Framework Core repository services with a specific <see cref="DataContext"/>> to the specified service collection.
        /// </summary>
        /// <typeparam name="TDataContext">The type of the DataContext.</typeparam>
        /// <returns>The service collection so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public IServiceCollection AddXEntityFrameworkRepositories<TDataContext>()
            where TDataContext : DataContext
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAddScoped<IRepository>(provider =>
            {
                var context = provider.GetRequiredService<TDataContext>();
                return new EntityFrameworkRepository(context);
            });

            services.TryAddScoped<IRepository<TDataContext>>(provider =>
            {
                var context = provider.GetRequiredService<TDataContext>();
                return new EntityFrameworkRepository<TDataContext>(context);
            });

            services.TryAddScoped<IUnitOfWork>(provider =>
            {
                var context = provider.GetRequiredService<TDataContext>();
                return new EntityFrameworkUnitOfWork(context, provider);
            });

            services.TryAddScoped<IUnitOfWork<TDataContext>>(provider =>
            {
                var context = provider.GetRequiredService<TDataContext>();
                return new EntityFrameworkUnitOfWork<TDataContext>(context, provider);
            });

            return services;
        }
    }
}