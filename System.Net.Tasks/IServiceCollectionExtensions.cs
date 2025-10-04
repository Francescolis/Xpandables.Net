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
using System.Net.Tasks;
using System.Net.Tasks.Pipelines;

using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Net.DependencyInjection;
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
        /// Adds Mediator services to the current <see cref="IServiceCollection"/> instance using the default <see
        /// cref="Mediator"/> implementation.
        /// </summary>
        /// <remarks>Call this method during application startup to enable XMediator-based request and
        /// notification handling. This overload registers the default mediator implementation; use the generic overload
        /// to specify a custom mediator type if needed.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with XMediator services registered.</returns>
        public IServiceCollection AddXMediator() =>
            services.AddXMediator<Mediator>();

        /// <summary>
        /// Adds the default dependency manager implementation to the service collection.
        /// </summary>
        /// <remarks>This method registers <see cref="DependencyManager"/> as the implementation for X
        /// dependency management. Call this method during application startup to enable X dependency
        /// features.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the X dependency manager registered.</returns>
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
    }
}
