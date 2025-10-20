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
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Cqrs;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring and managing <see cref="IRequestHandler{TRequest}"/>s within an IServiceCollection.
/// </summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class ICqrsExtensions
{
    internal readonly record struct HandlerType(Type Type, IEnumerable<Type> Interfaces);

    /// <summary>
    /// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The service collection to extend. Cannot be null.</param>"
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds the default dependency manager implementation to the service collection.
        /// </summary>
        /// <remarks>This method registers <see cref="DependencyManager"/> as the implementation for X
        /// dependency management. Call this method during application startup to enable X dependency
        /// features.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the dependency manager registered.</returns>
        public IServiceCollection AddXDependencyManager() =>
            services.AddXDependencyManager<DependencyManager>();

        /// <summary>
        /// Registers the specified dependency manager implementation as a scoped service in the dependency injection
        /// container.
        /// </summary>
        /// <remarks>Use this method to configure a custom dependency manager for the application. The
        /// registered implementation will be resolved as a scoped service for each request.</remarks>
        /// <typeparam name="TDependencyManager">The type of the dependency manager to register. Must implement <see cref="IDependencyManager"/> and have a
        /// public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the dependency manager registration added.</returns>
        public IServiceCollection AddXDependencyManager<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDependencyManager>()
            where TDependencyManager : class, IDependencyManager =>
            services.AddScoped<IDependencyManager, TDependencyManager>();

        /// <summary>
        /// Adds a scoped dependency provider of the specified type to the service collection.
        /// </summary>
        /// <typeparam name="TDependencyProvider">The type that implements the IDependencyProvider interface. Must have a public constructor.</typeparam>
        /// <returns>The IServiceCollection instance with the dependency provider registered.</returns>
        public IServiceCollection AddXDependencyProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDependencyProvider>()
            where TDependencyProvider : class, IDependencyProvider =>
            services.AddScoped<IDependencyProvider, TDependencyProvider>();

        /// <summary>
        /// Registers all sealed request handler types from the specified assemblies with the dependency injection container as
        /// transient services.
        /// </summary>
        /// <remarks>Handler types are identified by their implementation of supported handler interfaces,
        /// such as <see cref="IRequestHandler{TRequest}"/>, <see cref="IRequestContextHandler{TRequest}"/>, and related
        /// interfaces. Only sealed, non-abstract, non-generic classes are registered. Each handler interface is
        /// registered as a transient service. This method requires dynamic code generation and may require unreferenced
        /// code; see the method attributes for details.</remarks>
        /// <param name="assemblies">An array of assemblies to scan for handler implementations. If not specified or empty, the calling assembly
        /// is used.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance with handler services registered.</returns>
        [RequiresDynamicCode("Dynamic code generation is required for this method.")]
        [RequiresUnreferencedCode("Calls MakeGenericMethod which may require unreferenced code.")]
        public IServiceCollection AddXRequestHandlers(params Assembly[] assemblies)
        {
            assemblies = assemblies is { Length: > 0 } ? assemblies : [Assembly.GetCallingAssembly()];

            IEnumerable<HandlerType> handlerTypes = assemblies.SelectMany(assembly =>
                    assembly.GetTypes()
                        .Where(type =>
                            type is
                            {
                                IsClass: true,
                                IsAbstract: false,
                                IsSealed: true,
                                IsGenericType: false
                            }
                            && type.GetInterfaces().Any(i =>
                                i.IsGenericType &&
                                (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)
                                || i.GetGenericTypeDefinition() == typeof(IRequestContextHandler<>)
                                || i.GetGenericTypeDefinition() == typeof(IRequestContextHandler<,>)
                                || i.GetGenericTypeDefinition() == typeof(IRequestPostHandler<>)
                                || i.GetGenericTypeDefinition() == typeof(IDependencyRequestHandler<,>)
                                || i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>)
                                || i.GetGenericTypeDefinition() == typeof(IStreamRequestContextHandler<,>)
                                || i.GetGenericTypeDefinition() == typeof(IRequestPreHandler<>)))))
                .Select(type => new HandlerType(
                    type,
                    type.GetInterfaces()
                        .Where(i => i.IsGenericType &&
                                    (i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)
                                    || i.GetGenericTypeDefinition() == typeof(IRequestContextHandler<>)
                                    || i.GetGenericTypeDefinition() == typeof(IRequestContextHandler<,>)
                                    || i.GetGenericTypeDefinition() == typeof(IRequestPostHandler<>)
                                    || i.GetGenericTypeDefinition() == typeof(IDependencyRequestHandler<,>)
                                    || i.GetGenericTypeDefinition() == typeof(IStreamRequestHandler<,>)
                                    || i.GetGenericTypeDefinition() == typeof(IStreamRequestContextHandler<,>)
                                    || i.GetGenericTypeDefinition() == typeof(IRequestPreHandler<>)))));

            foreach (HandlerType handlerType in handlerTypes)
            {
                foreach (Type interfaceType in handlerType.Interfaces)
                {
                    _ = services.AddTransient(interfaceType, handlerType.Type);
                }
            }

            return services;
        }
    }
}
