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
#pragma warning disable IDE0130 // Namespace does not match folder structure
using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering XPipeline decorators and related event buffering services in an
/// IServiceCollection for event store, domain events, and integration outbox scenarios.
/// </summary>
/// <remarks>Use this static class to add XPipeline-based decorators and their dependencies to the dependency
/// injection container. The provided methods enable event decoration, domain event handling, and reliable integration
/// event dispatching by registering the necessary pipeline components and buffers. These extensions are intended to be
/// called during application startup to configure event processing pipelines in distributed or event-driven
/// applications.</remarks>
public static class IEventPipelineExtensions
{
    /// <summary>
    /// Registers the XPipeline event store event decorator in the dependency injection container.
    /// </summary>
    /// <remarks>This method adds the PipelineEventStoreEventDecorator to the service collection, allowing
    /// event store events to be processed through the XPipeline decorator mechanism. Use this method to enable event
    /// decoration in XPipeline-based event store scenarios.</remarks>
    /// <param name="services">The service collection to which the event store event decorator will be added. Cannot be null.</param>
    /// <returns>The same IServiceCollection instance, enabling method chaining.</returns>
    public static IServiceCollection AddXPipelineEventStoreEventDecorator(
        this IServiceCollection services)
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
    /// <param name="services">The service collection to which the domain events pipeline decorator and related services will be added. Cannot
    /// be null.</param>
    /// <returns>The same service collection instance, with the domain events pipeline decorator and its dependencies registered.</returns>
    public static IServiceCollection AddXPipelineDomainEventsDecorator(
        this IServiceCollection services)
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
    /// <param name="services">The service collection to which the outbox decorator and related services will be added. Cannot be null.</param>
    /// <returns>The same service collection instance, configured with the outbox decorator and event buffering services.</returns>
    public static IServiceCollection AddXPipelineIntegrationOutboxDecorator(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .AddXPipelineDecorator(typeof(PipelineIntegrationOutboxDecorator<>))
            .AddScoped<IPendingIntegrationEventsBuffer, PendingIntegrationEventsBuffer>();
    }

    /// <summary>
    /// Registers the aggregate store post handler for request processing in the dependency injection container.
    /// </summary>
    /// <remarks>This method adds a scoped implementation of <see cref="IRequestPostHandler{TRequest}"/> using
    /// <see cref="AggregateRequestPostHandler{TRequest}"/>. Call this method during application startup to enable
    /// aggregate post-processing for requests.</remarks>
    /// <param name="services">The service collection to which the aggregate store post handler will be added. Cannot be null.</param>
    /// <returns>The same service collection instance with the aggregate store post handler registered.</returns>
    public static IServiceCollection AddXAggregateStorePostHandler(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddScoped(typeof(IRequestPostHandler<>), typeof(AggregateRequestPostHandler<>));
    }

    /// <summary>
    /// Adds the aggregate dependency provider to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which the aggregate dependency provider will be added. Cannot be null.</param>
    /// <returns>The same instance of <see cref="IServiceCollection"/> that was provided, to support method chaining.</returns>
    public static IServiceCollection AddXAggregateDependencyProvider(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddXDependencyProvider<AggregateDependencyProvider>();
    }
}
