/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
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
using System.Events.Domain;
using System.Events.Integration;
using System.Pipelines;

namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering pipeline decorators with an <see cref="IServiceCollection"/>.
/// </summary>
public static class IPipelineExtensions
{
	/// <summary>
	/// Registers the XPipeline event store event decorator in the dependency injection container.
	/// </summary>
	/// <remarks>This method adds the PipelineEventStoreEventDecorator to the service collection, allowing
	/// event store events to be processed through the XPipeline decorator mechanism. Use this method to enable event
	/// decoration in XPipeline-based event store scenarios.</remarks>
	/// <returns>The same IServiceCollection instance, enabling method chaining.</returns>
	public static IServiceCollection AddXPipelineAfterCommitDomainEventDecorator(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);
		return services.AddXPipelineDecorator(typeof(PipelineAfterCommitDomainEventDecorator<>));
	}

	/// <summary>
	/// Registers the domain events pipeline decorator and its dependencies with the specified service collection.
	/// </summary>
	/// <remarks>This method adds the generic PipelineDomainEventsDecorator to the pipeline and registers the
	/// PendingDomainEventsBuffer for managing pending domain events. Call this method during application startup to
	/// enable domain event handling in the pipeline.</remarks>
	/// <returns>The same service collection instance, with the domain events pipeline decorator and its dependencies registered.</returns>
	public static IServiceCollection AddXPipelineBeforeCommitDomainEventDecorator(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);
		return services
			.AddXPipelineDecorator(typeof(PipelineBeforeCommitDomainEventDecorator<>))
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
	public static IServiceCollection AddXPipelineEnqueueIntegrationEventDecorator(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);
		return services
			.AddXPipelineDecorator(typeof(PipelineEnqueueIntegrationEventDecorator<>))
			.AddScoped<IPendingIntegrationEventsBuffer, PendingIntegrationEventsBuffer>();
	}
}
