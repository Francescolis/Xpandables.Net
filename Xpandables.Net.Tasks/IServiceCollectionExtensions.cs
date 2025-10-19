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

using Xpandables.Net.Tasks;
using Xpandables.Net.Tasks.Pipelines;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Xpandables.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
/// instance.
/// </summary>
/// <remarks>This static class contains helper methods that extend the functionality of <see
/// cref="IServiceCollection"/> to simplify service registration and setup in dependency injection scenarios. All
/// methods are intended to be used as extension methods and should be called on an existing <see
/// cref="IServiceCollection"/> object.</remarks>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IServiceCollectionExtensions
{
    internal readonly record struct HandlerType(Type Type, IEnumerable<Type> Interfaces);

    /// <summary>
    /// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The service collection to extend. Cannot be null.</param>"
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds Mediator and related pipeline request handler services to the current service collection.
        /// </summary>
        /// <remarks>it also registers the pipeline request handler services.
        /// <para>The registration of default pipeline decorators is not included in this method.
        /// You should add them in this order : </para>
        /// <list type="bullet">
        /// <item>ExceptionDecorator</item>
        /// <item>ValidationDecorator</item>
        /// <item>UnitOfWork</item>
        /// <item>DependencyDecorator</item>
        /// <item>PreDecorator</item>
        /// <item>PostDecorator</item>
        /// <item>DependencyManager</item>
        /// </list>
        /// In order to register custom pipeline decorators, use the <see cref="AddXPipelineDecorator(IServiceCollection, Type)"/> method.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with Mediator services registered. This enables further
        /// configuration of dependency injection.</returns>
        public IServiceCollection AddXMediator() =>
            services
                .AddXMediator<Mediator>()
                .AddXPipelineRequestHandler();

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
        /// Adds the Pipeline dependency decorator to the service collection, enabling dependency management within
        /// pipeline components.
        /// </summary>
        /// <remarks>Use this method to register the Pipeline dependency decorator when configuring
        /// services for pipeline-based processing. This allows pipeline components to resolve and manage dependencies
        /// automatically. Call this method during application startup as part of your dependency injection
        /// setup.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the Pipeline dependency decorator registered.</returns>
        public IServiceCollection AddXPipelineDependencyDecorator() =>
            services.AddXPipelineDecorator(typeof(PipelineDependencyDecorator<>));

        /// <summary>
        /// Adds the default post-processing decorator to the pipeline configuration for all registered pipeline
        /// handlers.
        /// </summary>
        /// <remarks>This method registers <see cref="PipelinePostDecorator{TRequest}"/> as a decorator
        /// for all pipeline handler types. Call this method during service configuration to enable post-processing
        /// behavior in the pipeline.</remarks>
        /// <returns>An <see cref="IServiceCollection"/> containing the service registrations, including the post-processing
        /// decorator.</returns>
        public IServiceCollection AddXPipelinePostDecorator() =>
            services.AddXPipelineDecorator(typeof(PipelinePostDecorator<>));

        /// <summary>
        /// Adds the default pipeline request handler implementation to the service collection for dependency injection.
        /// </summary>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the pipeline request handler registered.</returns>
        public IServiceCollection AddXPipelineRequestHandler()
            => services.AddXPipelineRequestHandler(typeof(PipelineRequestHandler<>));

        /// <summary>
        /// Registers the default pre-decorator for Pipeline into the service collection.
        /// </summary>
        /// <remarks>This method adds the generic <c>PipelinePreDecorator&lt;T&gt;</c> to the service
        /// collection, enabling pre-processing behavior for Pipeline handlers. Call this method during application
        /// startup to configure the pipeline decorators before resolving pipeline services.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the XPipeline pre-decorator registered.</returns>
        public IServiceCollection AddXPipelinePreDecorator() =>
            services.AddXPipelineDecorator(typeof(PipelinePreDecorator<>));

        /// <summary>
        /// Adds the PipelineExceptionDecorator to the pipeline, enabling exception handling for pipeline operations.
        /// </summary>
        /// <remarks>This method registers the PipelineExceptionDecorator for all pipeline types. Use this
        /// to ensure that exceptions thrown during pipeline execution are handled consistently across the
        /// application.</remarks>
        /// <returns>The same IServiceCollection instance, allowing for method chaining.</returns>
        public IServiceCollection AddXPipelineExceptionDecorator() =>
            services.AddXPipelineDecorator(typeof(PipelineExceptionDecorator<>));

        /// <summary>
        /// Registers the specified mediator implementation as a scoped service for dependency injection.
        /// </summary>
        /// <remarks>Use this method to configure a custom mediator implementation for use within the
        /// application's dependency injection container. The mediator will be resolved as a scoped service, meaning a
        /// new instance is created per request or scope.</remarks>
        /// <typeparam name="TMediator">The type of the mediator to register. Must be a class that implements <see cref="IMediator"/> and have a
        /// public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the mediator service registration added.</returns>
        public IServiceCollection AddXMediator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMediator>()
            where TMediator : class, IMediator =>
            services.AddScoped<IMediator, TMediator>();

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
        /// Registers a pipeline request handler of the specified type with the dependency injection container.
        /// </summary>
        /// <remarks>Use this method to add custom pipeline request handlers to the service collection for
        /// dependency injection. The handler will be registered with transient lifetime, meaning a new instance is
        /// created each time it is requested.</remarks>
        /// <param name="type">The type that implements the <see cref="IPipelineRequestHandler{TRequest}"/> interface. Must have public constructors and
        /// implement the required interface.</param>
        /// <returns>The IServiceCollection instance with the pipeline request handler registered as a transient service.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified type does not implement the <see cref="IPipelineRequestHandler{TRequest}"/>> interface.</exception>
        public IServiceCollection AddXPipelineRequestHandler([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            ArgumentNullException.ThrowIfNull(services);

            if (!type.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IPipelineRequestHandler<>)))
            {
                throw new InvalidOperationException(
                    $"{type.Name} does not implement IPipelineRequestHandler<,> interface.");
            }

            return services.AddTransient(typeof(IPipelineRequestHandler<>), type);
        }

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
