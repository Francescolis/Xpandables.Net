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

using Xpandables.Net.Tasks;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for registering mediator implementations and configuring event sourcing pipeline
/// decorators within a dependency injection container.
/// </summary>
/// <remarks>Use the methods in this class to enable advanced mediator features, such as event sourcing,
/// validation, exception handling, outbox integration, unit of work, domain events, dependency management, and request
/// pre/post processing. These extensions are intended to be called during application startup to configure the request
/// handling pipeline.</remarks>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class ITaskExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the specified mediator implementation and configures event sourcing pipeline decorators for
        /// request handling within the dependency injection container.
        /// </summary>
        /// <remarks>This method adds a scoped mediator service and configures a series of pipeline
        /// decorators to enable event sourcing, validation, exception handling, outbox integration, unit of work,
        /// domain events, dependency management, and pre/post processing for requests. Call this method during
        /// application startup to enable advanced mediator features with event sourcing support.</remarks>
        /// <typeparam name="TMediator">The type of mediator to register. Must be a class implementing <see cref="IMediator"/> and have a public
        /// constructor.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the mediator and event sourcing pipeline decorators
        /// registered.</returns>
        public IServiceCollection AddXMediatorWithEventSourcing<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMediator>()
            where TMediator : class, IMediator =>
            services
                .AddScoped<IMediator, TMediator>()
                .AddXPipelineExceptionDecorator()
                .AddXPipelineValidationDecorator()
                .AddXPipelineIntegrationOutboxDecorator()
                .AddXPipelineUnitOfWorkDecorator()
                .AddXPipelineDomainEventsDecorator()
                .AddXPipelineDependencyDecorator()
                .AddXPipelinePreDecorator()
                .AddXPipelinePostDecorator()
                .AddXPipelineRequestHandler()
                .AddXDependencyManager();

        /// <summary>
        /// Adds Mediator services with event sourcing support to the current service collection.
        /// </summary>
        /// <remarks>This method registers the default <c>Mediator</c> implementation for event sourcing.
        /// Call this method during application startup to enable event-driven processing and event persistence features
        /// provided by XMediator.</remarks>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with Mediator and event sourcing services registered.</returns>
        public IServiceCollection AddXMediatorWithEventSourcing() =>
            services.AddXMediatorWithEventSourcing<Mediator>();
    }
}
