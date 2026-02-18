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
using System.Cache;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering event store and outbox store services with an <see
/// cref="IServiceCollection"/> in applications using the XEvent and Outbox patterns.
/// </summary>
/// <remarks>These extension methods simplify the configuration of event sourcing and outbox services by
/// registering default or custom implementations with the dependency injection container. Use these methods to add
/// support for event storage and outbox processing in your application's service pipeline.</remarks>
public static class PrimitivesExtensions
{
	extension(IServiceCollection services)
	{
		/// <summary>
		/// Registers a singleton implementation of the specified cache type resolver in the service collection.
		/// </summary>
		/// <remarks>Use this method to configure dependency injection for event cache type resolution. If
		/// an IEventCacheTypeResolver is already registered, this method will not overwrite the existing
		/// registration.</remarks>
		/// <typeparam name="TCacheTypeResolver">The type of cache type resolver to register. Must implement the ICacheTypeResolver interface
		/// and have a public constructor.</typeparam>
		/// <returns>The IServiceCollection instance with the cache type resolver registered.</returns>
		public IServiceCollection AddXCacheTypeResolver<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCacheTypeResolver>()
			where TCacheTypeResolver : class, ICacheTypeResolver
		{
			ArgumentNullException.ThrowIfNull(services);
			services.TryAddSingleton<ICacheTypeResolver, TCacheTypeResolver>();
			return services;
		}

		/// <summary>
		/// Adds the specified cache type resolver implementation to the service collection as a singleton.
		/// </summary>
		/// <remarks>If an ICacheTypeResolver service is already registered, this method does not
		/// overwrite the existing registration.</remarks>
		/// <param name="cacheTypeResolver">The type that implements the ICacheTypeResolver interface. Must have a public constructor.</param>
		/// <returns>The IServiceCollection instance for chaining additional configuration calls.</returns>
		public IServiceCollection AddXCacheTypeResolver([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type cacheTypeResolver)
		{
			ArgumentNullException.ThrowIfNull(services);
			services.TryAddSingleton(typeof(ICacheTypeResolver), cacheTypeResolver);
			return services;
		}

		/// <summary>
		/// Adds the default implementation of the ICacheTypeResolver service to the service collection.
		/// </summary>
		/// <remarks>This method registers CacheTypeResolver as a singleton service for the
		/// ICacheTypeResolver interface if it has not already been registered. Call this method during application
		/// startup to enable type resolution for caching scenarios.</remarks>
		/// <returns>The IServiceCollection instance with the ICacheTypeResolver service registered.</returns>
		public IServiceCollection AddXCacheTypeResolver()
		{
			ArgumentNullException.ThrowIfNull(services);
			services.TryAddSingleton<ICacheTypeResolver, CacheTypeResolver>();
			return services;
		}

		/// <summary>
		/// Adds the default cache type resolver to the service collection.
		/// </summary>
		/// <param name="assemblies">An optional array of assemblies to register for type resolution. If no assemblies are provided, all non-legacy
		/// assemblies will be registered.</param>
		/// <remarks>This method registers the CacheTypeResolver as the implementation for
		/// cache type resolution. Call this method during application startup to enable cache type resolution
		/// services.</remarks>
		/// <returns>The same IServiceCollection instance, allowing for method chaining.</returns>
		[RequiresUnreferencedCode("Uses reflection to load types from assemblies.")]
		public IServiceCollection AddXCacheTypeResolver(params Assembly[] assemblies)
		{
			ArgumentNullException.ThrowIfNull(services);
			services.AddXCacheTypeResolver();
			CacheTypeResolver.RegisterAssemblies(assemblies);
			return services;
		}

		/// <summary>
		/// Registers a type resolver for Cache that scans the specified assemblies and filters types using the provided
		/// predicate.
		/// </summary>
		/// <remarks>This method uses reflection to load types from assemblies, which may impact trimming and linking
		/// scenarios. The resolver will only include types that satisfy the specified predicate.</remarks>
		/// <param name="predicate">A delegate that determines whether a given type should be included by the resolver. Cannot be null.</param>
		/// <param name="assemblies">The assemblies to scan for types. At least one assembly must be provided.</param>
		/// <returns>The service collection with the Cache type resolver registered. This enables further chaining of service
		/// configuration.</returns>
		[RequiresUnreferencedCode("Uses reflection to load types from assemblies.")]
		public IServiceCollection AddXCacheTypeResolver(Predicate<Type> predicate, params Assembly[] assemblies)
		{
			ArgumentNullException.ThrowIfNull(services);
			services.AddXCacheTypeResolver();
			CacheTypeResolver.RegisterAssemblies(predicate, assemblies);
			return services;
		}
	}
}
