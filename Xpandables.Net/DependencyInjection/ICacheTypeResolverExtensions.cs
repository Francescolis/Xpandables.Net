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
using System.Reflection;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring cache type resolution within an application's dependency injection
/// container.
/// </summary>
/// <remarks>This static class contains extension methods intended to simplify the registration and setup of cache
/// type resolvers using the IServiceCollection interface. These methods are typically used during application startup
/// to enable caching features or customize cache behavior.</remarks>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class ICacheTypeResolverExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds a singleton implementation of <see cref="ICacheTypeResolver"/> to the service collection using the
        /// specified type.
        /// </summary>
        /// <remarks>Use this method to configure dependency injection for cache type resolution.
        /// Subsequent calls will replace any previously registered <see cref="ICacheTypeResolver"/>
        /// implementation.</remarks>
        /// <typeparam name="TCacheTypeResolver">The type that implements <see cref="ICacheTypeResolver"/> and will be registered as a singleton service.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the <see cref="ICacheTypeResolver"/> service registered.</returns>
        public IServiceCollection AddXCacheTypeResolver<TCacheTypeResolver>()
            where TCacheTypeResolver : class, ICacheTypeResolver
        {
            services.AddSingleton<ICacheTypeResolver, TCacheTypeResolver>();
            return services;
        }

        /// <summary>
        /// Registers an <see cref="ICacheTypeResolver"/> implementation that scans the specified assemblies for
        /// cacheable types and adds it to the service collection.
        /// </summary>
        /// <remarks>Use this method to enable type-based cache resolution for types defined in the
        /// provided assemblies. The registered <see cref="ICacheTypeResolver"/> will be available as a singleton
        /// service.</remarks>
        /// <param name="assemblies">An array of assemblies to scan for types that can be resolved by the cache type resolver. Cannot be null.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance for chaining additional service registrations.</returns>
        public IServiceCollection AddXCacheTypeResolver(params Assembly[] assemblies)
        {
            services.AddSingleton<ICacheTypeResolver, CacheTypeResolver>(provider =>
            {
                var memoryCache = provider.GetRequiredService<IMemoryCache>();
                var resolver = new CacheTypeResolver(memoryCache);
                resolver.RegisterAssemblies(assemblies);
                return resolver;
            });
            return services;
        }
    }
}
