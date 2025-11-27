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
using System.OperationResults.Tasks;

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
public static class ITaskExtensions
{
    /// <summary>
    /// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
    /// </summary>
    /// <param name="services">The service collection to extend. Cannot be null.</param>"
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds Mediator and related pipeline request handler services to the current service collection.
        /// </summary>
        /// <remarks>
        /// if you want to add pipeline decorators, register handler services in this order :
        /// <list type="bullet">
        /// <item>PipelinePreDecorator</item>
        /// <item>PipelinePostDecorator</item>
        /// <item>PipelineUnitOfWorkDecorator</item>
        /// <item>PipelineValidationDecorator</item>
        /// <item>PipelineExceptionDecorator</item>
        /// <item>PipelineRequestHandler</item>
        /// </list>
        /// <para>In order to register the mediator to be used with Event sourcing, add registrations as follow:</para>
        /// <list type="bullet">
        /// <item>PipelineDomainEventsDecorator</item>
        /// <item>PipelineIntegrationOutboxDecorator</item>
        /// <item>PipelinePreDecorator</item>
        /// <item>PipelinePostDecorator</item>
        /// <item>PipelineUnitOfWorkDecorator</item>
        /// <item>PipelineEventStoreEventDecorator</item>
        /// <item>PipelineValidationDecorator</item>
        /// <item>PipelineExceptionDecorator</item>
        /// <item>PipelineRequestHandler</item>
        /// </list>
        /// In order to register custom pipeline decorators, use the <see langword="AddXPipelineDecorator(IServiceCollection, Type)"/> method.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with Mediator services registered. This enables further
        /// configuration of dependency injection.</returns>
        public IServiceCollection AddXMediator() => services.AddXMediator<Mediator>();

        /// <summary>
        /// Adds Mediator and configures the request pipeline with pre-processing, post-processing, validation, and
        /// exception handling decorators.
        /// </summary>
        /// <remarks>Call this method during application startup to register Mediator and its pipeline
        /// decorators in the dependency injection container. The decorators provide extensibility points for request
        /// validation, exception handling, and additional pre- and post-processing logic. This method is intended to be
        /// used as part of the service configuration in ASP.NET Core or similar applications.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with Mediator and pipeline decorators registered. This
        /// enables mediator-based request handling with extensible pipeline behaviors.</returns>
        public IServiceCollection AddXMediatorWithPipelines()
            => services
                .AddXMediator()
                .AddXPipelinePreDecorator()
                .AddXPipelinePostDecorator()
                .AddXPipelineUnitOfWorkDecorator()
                .AddXPipelineValidationDecorator()
                .AddXPipelineExceptionDecorator();

        /// <summary>
        /// Configures Mediator with event sourcing pipelines and related decorators for domain events, integration
        /// outbox, validation, exception handling, and event store integration.
        /// </summary>
        /// <remarks>Call this method during application startup to ensure that Mediator and all required
        /// event sourcing pipeline decorators are registered. The returned IServiceCollection can be used for further
        /// service registrations. This method is intended to be used in applications that require event sourcing,
        /// domain event handling, and integration outbox patterns.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with Mediator and event sourcing pipeline decorators
        /// registered. This enables event-driven processing and enhanced pipeline behaviors within the application's
        /// dependency injection container.</returns>
        public IServiceCollection AddXMediatorWithEventSourcingPipelines()
            => services
                .AddXMediator()
                .AddXPipelineDomainEventsDecorator()
                .AddXPipelineIntegrationOutboxDecorator()
                .AddXPipelinePreDecorator()
                .AddXPipelinePostDecorator()
                .AddXPipelineUnitOfWorkDecorator()
                .AddXPipelineEventStoreEventDecorator()
                .AddXPipelineValidationDecorator()
                .AddXPipelineExceptionDecorator();

        /// <summary>
        /// Registers the specified mediator implementation as a scoped service for dependency injection.
        /// </summary>
        /// <remarks>Use this method to configure a custom mediator implementation for use within the
        /// application's dependency injection container. The mediator will be resolved as a scoped service, meaning a
        /// new instance is created per request or scope.
        /// <para></para>if you want to add pipeline decorators, register handler services in this order :
        /// <list type="bullet">
        /// <item>PipelinePreDecorator</item>
        /// <item>PipelinePostDecorator</item>
        /// <item>PipelineUnitOfWorkDecorator</item>
        /// <item>PipelineValidationDecorator</item>
        /// <item>PipelineExceptionDecorator</item>
        /// <item>PipelineRequestHandler</item>
        /// </list>
        /// <para>In order to register the mediator to be used with Event sourcing, add registrations as follow:</para>
        /// <list type="bullet">
        /// <item>PipelineDomainEventsDecorator</item>
        /// <item>PipelineIntegrationOutboxDecorator</item>
        /// <item>PipelinePreDecorator</item>
        /// <item>PipelinePostDecorator</item>
        /// <item>PipelineUnitOfWorkDecorator</item>
        /// <item>PipelineEventStoreEventDecorator</item>
        /// <item>PipelineValidationDecorator</item>
        /// <item>PipelineExceptionDecorator</item>
        /// <item>PipelineRequestHandler</item>
        /// </list>
        /// </remarks>
        /// <typeparam name="TMediator">The type of the mediator to register. Must be a class that implements <see cref="IMediator"/> and have a
        /// public constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the mediator service registration added.</returns>
        public IServiceCollection AddXMediator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMediator>()
            where TMediator : class, IMediator =>
            services.AddScoped<IMediator, TMediator>()
            .AddXPipelineRequestHandler();
    }
}