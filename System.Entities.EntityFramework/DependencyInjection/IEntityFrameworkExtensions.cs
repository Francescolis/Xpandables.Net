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
using System.Entities.EntityFramework;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Extension methods for registering Entity Framework repository services.
/// </summary>
public static class IEntityFrameworkExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds default unit of work services with a specific <see cref="DataContext"/>> to the specified service collection.
        /// </summary>
        /// <typeparam name="TDataContext">The type of the DataContext.</typeparam>
        /// <returns>The service collection so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public IServiceCollection AddXEntityUnitOfWork<TDataContext>()
            where TDataContext : DataContext
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddXEntityUnitOfWork<IEntityUnitOfWork<TDataContext>, EntityUnitOfWork<TDataContext>>();
            services.AddScoped<IEntityUnitOfWork>(provider => provider.GetRequiredService<IEntityUnitOfWork<TDataContext>>());

            return services;
        }

        /// <summary>
        /// Adds the default repository implementation to the service collection.
        /// </summary>
        /// <param name="lifetime">The service lifetime.</param>
        /// <returns>The service collection so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
        public IServiceCollection AddXEntityRepository(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.TryAdd(new ServiceDescriptor(typeof(IEntityRepository<>), typeof(EntityRepository<>), lifetime));

            return services;
        }
    }
}
