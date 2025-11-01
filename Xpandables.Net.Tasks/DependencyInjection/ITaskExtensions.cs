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

using Xpandables.Net.Requests.Pipelines;
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
public static class ITaskExtensions
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
        /// <item>PreDecorator</item>
        /// <item>PostDecorator</item>
        /// </list>
        /// In order to register custom pipeline decorators, use the <see langword="AddXPipelineDecorator(IServiceCollection, Type)"/> method.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with Mediator services registered. This enables further
        /// configuration of dependency injection.</returns>
        public IServiceCollection AddXMediator() =>
            services
                .AddXMediator<Mediator>()
                .AddXPipelinePreDecorator()
                .AddXPipelinePostDecorator()
                .AddXPipelineValidationDecorator()
                .AddXPipelineExceptionDecorator()
                .AddXPipelineRequestHandler();

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
        /// Adds a pipeline validation decorator to the service collection for use with Pipeline requests.
        /// </summary>
        /// <remarks>This method registers the <c>PipelineValidationDecorator&lt;TRequest&gt;</c> in the
        /// service collection, enabling automatic validation of requests processed through the XPipeline. Call this
        /// method during application startup to ensure validation is applied to all pipeline requests.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the pipeline validation decorator registered.</returns>
        public IServiceCollection AddXPipelineValidationDecorator() =>
            services.AddXPipelineDecorator(typeof(PipelineValidationDecorator<>));

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
    }
}