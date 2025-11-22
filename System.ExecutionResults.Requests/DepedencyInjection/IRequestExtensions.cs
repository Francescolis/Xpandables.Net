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
using System.ExecutionResults;
using System.ExecutionResults.Pipelines;
using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
/// instance.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of <see
/// cref="IServiceCollection"/> to simplify service registration and setup in dependency injection scenarios. All
/// methods are intended to be used as extension methods and should be called on an existing <see
/// cref="IServiceCollection"/> object.</remarks>
public static class IRequestExtensions
{
    internal readonly record struct HandlerType(Type Type, IEnumerable<Type> Interfaces);

    /// <summary>
    /// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The service collection to extend. Cannot be null.</param>"
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the specified pipeline decorator type as a transient implementation of the <see cref="IPipelineDecorator{TRequest}"/>
        /// interface in the service collection.
        /// </summary>
        /// <remarks>Use this method to add custom pipeline decorators to the dependency injection
        /// container. The decorator type must implement the <see cref="IPipelineDecorator{TRequest}"/> interface to be registered
        /// successfully.</remarks>
        /// <param name="pipeline">The type of the pipeline decorator to register. Must implement the <see cref="IPipelineDecorator{TRequest}"/> interface and have
        /// public constructors.</param>
        /// <returns>The updated IServiceCollection instance with the pipeline decorator registered.</returns>
        /// <exception cref="InvalidOperationException">Thrown if pipeline does not implement the <see cref="IPipelineDecorator{TRequest}"/> interface.</exception>
        public IServiceCollection AddXPipelineDecorator([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] Type pipeline)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(services);

            if (!pipeline.GetInterfaces().Any(i =>
                    i.IsGenericType
                    && i.GetGenericTypeDefinition() == typeof(IPipelineDecorator<>)))
            {
                throw new InvalidOperationException(
                    $"{pipeline.Name} does not implement IPipelineDecorator<,> interface.");
            }

            return services.AddTransient(typeof(IPipelineDecorator<>), pipeline);
        }

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