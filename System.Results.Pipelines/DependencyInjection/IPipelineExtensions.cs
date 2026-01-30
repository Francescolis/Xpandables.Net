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
using System.Events.Domain;
using System.Events.Integration;
using System.Results.Pipelines;

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
public static class IPipelineExtensions
{
    /// <summary>
    /// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The service collection to extend. Cannot be null.</param>"
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the PipelineUnitOfWorkDecorator for all pipeline handlers in the service collection, enabling
        /// unit-of-work behavior within the pipeline execution.
        /// </summary>
        /// <remarks>Use this method to ensure that each pipeline handler is executed within a
        /// unit-of-work scope, which can help manage transactional consistency and resource cleanup. This method should
        /// be called during application startup as part of dependency injection configuration.</remarks>
        /// <returns>The IServiceCollection instance with the PipelineUnitOfWorkDecorator registered. This enables further
        /// chaining of service registrations.</returns>
        public IServiceCollection AddXPipelineUnitOfWorkDecorator() =>
            services.AddXPipelineDecorator(typeof(PipelineUnitOfWorkDecorator<>));

        /// <summary>
        /// Registers the PipelineDataUnitOfWorkDecorator for all pipeline handlers, enabling ADO.NET
        /// transaction management within the pipeline execution.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this method to ensure that each pipeline handler is executed within an ADO.NET transaction scope.
        /// The transaction will be committed on success or rolled back on failure.
        /// </para>
        /// <para>
        /// This is the ADO.NET equivalent of <see cref="AddXPipelineUnitOfWorkDecorator"/> which is used
        /// for Entity Framework Core persistence.
        /// </para>
        /// </remarks>
        /// <returns>The IServiceCollection instance with the PipelineDataUnitOfWorkDecorator registered.</returns>
        public IServiceCollection AddXPipelineDataUnitOfWorkDecorator() =>
            services.AddXPipelineDecorator(typeof(PipelineDataUnitOfWorkDecorator<>));

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
        /// Registers the XPipeline event store event decorator in the dependency injection container.
        /// </summary>
        /// <remarks>This method adds the PipelineEventStoreEventDecorator to the service collection, allowing
        /// event store events to be processed through the XPipeline decorator mechanism. Use this method to enable event
        /// decoration in XPipeline-based event store scenarios.</remarks>
        /// <returns>The same IServiceCollection instance, enabling method chaining.</returns>
        public IServiceCollection AddXPipelineEventStoreEventDecorator()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddXPipelineDecorator(typeof(PipelineEventStoreEventDecorator<>));
        }

        /// <summary>
        /// Registers the domain events pipeline decorator and its dependencies with the specified service collection.
        /// </summary>
        /// <remarks>This method adds the generic PipelineDomainEventsDecorator to the pipeline and registers the
        /// PendingDomainEventsBuffer for managing pending domain events. Call this method during application startup to
        /// enable domain event handling in the pipeline.</remarks>
        /// <returns>The same service collection instance, with the domain events pipeline decorator and its dependencies registered.</returns>
        public IServiceCollection AddXPipelineDomainEventsDecorator()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services
                .AddXPipelineDecorator(typeof(PipelineDomainEventsDecorator<>))
                .AddScoped<IPendingDomainEventsBuffer, PendingDomainEventsBuffer>();
        }

        /// <summary>
        /// Adds the outbox decorator for pipeline integration to the service collection, enabling buffering and reliable
        /// dispatch of integration events within the pipeline.
        /// </summary>
        /// <remarks>This method registers the outbox decorator and a scoped buffer for pending integration
        /// events, supporting reliable event handling in distributed systems. Call this method during application startup
        /// to ensure integration events are buffered and dispatched as part of the pipeline execution.</remarks>
        /// <returns>The same service collection instance, configured with the outbox decorator and event buffering services.</returns>
        public IServiceCollection AddXPipelineIntegrationOutboxDecorator()
        {
            ArgumentNullException.ThrowIfNull(services);
            return services
                .AddXPipelineDecorator(typeof(PipelineIntegrationOutboxDecorator<>))
                .AddScoped<IPendingIntegrationEventsBuffer, PendingIntegrationEventsBuffer>();
        }

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

            var handlerInterface = type.GetInterfaces().FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IPipelineRequestHandler<>));

            if (handlerInterface is null)
            {
                throw new InvalidOperationException(
                    $"{type.Name} does not implement IPipelineRequestHandler<> interface.");
            }

            // If `type` is open generic (or implements handler as open generic), register open generic service.
            if (type.ContainsGenericParameters || handlerInterface.ContainsGenericParameters)
            {
                return services.AddTransient(typeof(IPipelineRequestHandler<>), type);
            }

            // Closed implementation: register specifically for the closed TRequest (IPipelineRequestHandler<TRequest>).
            var requestType = handlerInterface.GenericTypeArguments[0];
            var serviceType = typeof(IPipelineRequestHandler<>).MakeGenericType(requestType);

            return services.AddTransient(serviceType, type);
        }
    }
}