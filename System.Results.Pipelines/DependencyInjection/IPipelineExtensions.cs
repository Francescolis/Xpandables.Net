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
using System.Diagnostics.CodeAnalysis;
using System.Events.Domain;
using System.Events.Integration;
using System.Results.Pipelines;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering pipeline decorators with an <see cref="IServiceCollection"/>.
/// </summary>
/// <remarks>
/// <para>Decorators execute in <strong>registration order</strong> (first registered = outermost).
/// The recommended registration order is:</para>
/// <list type="number">
///   <item><see cref="AddXPipelineExceptionDecorator"/> — catches unhandled exceptions (outermost)</item>
///   <item><see cref="AddXPipelineValidationDecorator"/> — validates the request</item>
///   <item><see cref="AddXPipelinePreHanderDecorator"/> — pre-processing hooks</item>
///   <item><see cref="AddXPipelinePostHanderDecorator"/> — post-processing hooks</item>
///   <item><see cref="AddXPipelinePublishDomainEventDecorator"/> — publishes domain events</item>
///   <item><see cref="AddXPipelineCommitDomainEventDecorator"/> — commits domain events to the store</item>
///   <item><see cref="AddXPipelineEnqueueIntegrationEventDecorator"/> — enqueues outbox events</item>
///   <item><see cref="AddXPipelineRequireEntityUnitOfWorkDecorator"/> or
///         <see cref="AddXPipelineRequireDataUnitOfWorkDecorator"/> — commits the unit of work (innermost)</item>
/// </list>
/// <para>The exception decorator should always be the outermost layer so that any exception thrown
/// by inner decorators or the handler is captured. Call <see cref="ValidateXPipelineRegistration"/>
/// after all decorators are registered to verify at startup that the exception decorator is present.</para>
/// </remarks>
public static class IPipelineExtensions
{
	/// <summary>
	/// Provides extension methods for configuring and managing services within an <see cref="IServiceCollection"/>
	/// </summary>
	/// <param name="services">The service collection to extend. Cannot be null.</param>"
	extension(IServiceCollection services)
	{
		/// <summary>
		/// Registers the PipelineEntityUnitOfWorkDecorator for all pipeline handlers in the service collection, enabling
		/// unit-of-work behavior within the pipeline execution.
		/// </summary>
		/// <remarks>Use this method to ensure that each pipeline handler is executed within a
		/// unit-of-work scope, which can help manage transactional consistency and resource cleanup. This method should
		/// be called during application startup as part of dependency injection configuration.</remarks>
		/// <returns>The IServiceCollection instance with the PipelineEntityUnitOfWorkDecorator registered. This enables further
		/// chaining of service registrations.</returns>
		public IServiceCollection AddXPipelineRequireEntityUnitOfWorkDecorator() =>
			services.AddXPipelineDecorator(typeof(PipelineRequireEntityUnitOfWorkDecorator<>));

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
		/// This is the ADO.NET equivalent of <see cref="AddXPipelineRequireEntityUnitOfWorkDecorator"/> which is used
		/// for Entity Framework Core persistence.
		/// </para>
		/// </remarks>
		/// <returns>The IServiceCollection instance with the PipelineDataUnitOfWorkDecorator registered.</returns>
		public IServiceCollection AddXPipelineRequireDataUnitOfWorkDecorator() =>
			services.AddXPipelineDecorator(typeof(PipelineRequireDataUnitOfWorkDecorator<>));

		/// <summary>
		/// Adds the default post-processing decorator to the pipeline configuration for all registered pipeline
		/// handlers.
		/// </summary>
		/// <remarks>This method registers <see cref="PipelinePostHandlerDecorator{TRequest}"/> as a decorator
		/// for all pipeline handler types. Call this method during service configuration to enable post-processing
		/// behavior in the pipeline.</remarks>
		/// <returns>An <see cref="IServiceCollection"/> containing the service registrations, including the post-processing
		/// decorator.</returns>
		public IServiceCollection AddXPipelinePostHanderDecorator() =>
			services.AddXPipelineDecorator(typeof(PipelinePostHandlerDecorator<>));

		/// <summary>
		/// Registers the default pre-decorator for Pipeline into the service collection.
		/// </summary>
		/// <remarks>This method adds the generic <c>PipelinePreDecorator&lt;T&gt;</c> to the service
		/// collection, enabling pre-processing behavior for Pipeline handlers. Call this method during application
		/// startup to configure the pipeline decorators before resolving pipeline services.</remarks>
		/// <returns>The updated <see cref="IServiceCollection"/> instance with the XPipeline pre-decorator registered.</returns>
		public IServiceCollection AddXPipelinePreHanderDecorator() =>
			services.AddXPipelineDecorator(typeof(PipelinePreHandlerDecorator<>));

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
		public IServiceCollection AddXPipelineCommitDomainEventDecorator()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services.AddXPipelineDecorator(typeof(PipelineCommitDomainEventDecorator<>));
		}

		/// <summary>
		/// Registers the domain events pipeline decorator and its dependencies with the specified service collection.
		/// </summary>
		/// <remarks>This method adds the generic PipelineDomainEventsDecorator to the pipeline and registers the
		/// PendingDomainEventsBuffer for managing pending domain events. Call this method during application startup to
		/// enable domain event handling in the pipeline.</remarks>
		/// <returns>The same service collection instance, with the domain events pipeline decorator and its dependencies registered.</returns>
		public IServiceCollection AddXPipelinePublishDomainEventDecorator()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services
				.AddXPipelineDecorator(typeof(PipelinePublishDomainEventDecorator<>))
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
		public IServiceCollection AddXPipelineEnqueueIntegrationEventDecorator()
		{
			ArgumentNullException.ThrowIfNull(services);
			return services
				.AddXPipelineDecorator(typeof(PipelineEnqueueIntegrationEventDecorator<>))
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
		/// dependency injection. The handler will be registered with scoped lifetime to amortize the cost of
		/// pipeline construction and to align with scoped decorator dependencies.</remarks>
		/// <param name="type">The type that implements the <see cref="IPipelineRequestHandler{TRequest}"/> interface. Must have public constructors and
		/// implement the required interface.</param>
		/// <returns>The IServiceCollection instance with the pipeline request handler registered as a scoped service.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the specified type does not implement the <see cref="IPipelineRequestHandler{TRequest}"/>> interface.</exception>
		public IServiceCollection AddXPipelineRequestHandler([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
		{
			ArgumentNullException.ThrowIfNull(type);
			ArgumentNullException.ThrowIfNull(services);

			Type handlerInterface = type.GetInterfaces().FirstOrDefault(i =>
				i.IsGenericType &&
				i.GetGenericTypeDefinition() == typeof(IPipelineRequestHandler<>))
				?? throw new InvalidOperationException(
					$"{type.Name} does not implement IPipelineRequestHandler<> interface.");

			// If `type` is open generic (or implements handler as open generic), register open generic service.
			if (type.ContainsGenericParameters || handlerInterface.ContainsGenericParameters)
			{
				return services.AddScoped(typeof(IPipelineRequestHandler<>), type);
			}

			// Closed implementation: register specifically for the closed TRequest (IPipelineRequestHandler<TRequest>).
			Type requestType = handlerInterface.GenericTypeArguments[0];
			Type serviceType = typeof(IPipelineRequestHandler<>).MakeGenericType(requestType);

			return services.AddScoped(serviceType, type);
		}

		/// <summary>
		/// Validates that the pipeline decorator registrations include the exception decorator.
		/// </summary>
		/// <remarks>
		/// <para>Call this method after all pipeline decorators have been registered to verify that
		/// <see cref="PipelineExceptionDecorator{TRequest}"/> is present. Without it, unhandled exceptions
		/// will propagate directly to the caller instead of being transformed into a failure result.</para>
		/// <para>If the exception decorator is missing, this method logs a warning via the
		/// <paramref name="onWarning"/> callback (or throws <see cref="InvalidOperationException"/>
		/// when no callback is provided).</para>
		/// </remarks>
		/// <param name="onWarning">An optional callback invoked with a warning message when the exception
		/// decorator is missing. When <see langword="null"/>, an <see cref="InvalidOperationException"/> is thrown.</param>
		/// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
		/// <exception cref="InvalidOperationException">Thrown when the exception decorator is not registered
		/// and no <paramref name="onWarning"/> callback is provided.</exception>
		public IServiceCollection ValidateXPipelineRegistration(Action<string>? onWarning = null)
		{
			ArgumentNullException.ThrowIfNull(services);

			bool hasExceptionDecorator = services.Any(sd =>
				sd.ServiceType == typeof(IPipelineDecorator<>) &&
				sd.ImplementationType is { IsGenericTypeDefinition: true } impl &&
				impl == typeof(PipelineExceptionDecorator<>));

			if (!hasExceptionDecorator)
			{
				const string message =
					"PipelineExceptionDecorator is not registered. Without it, unhandled exceptions " +
					"will propagate to the caller instead of being captured as a Result. " +
					"Call AddXPipelineExceptionDecorator() before other decorators.";

				if (onWarning is not null)
				{
					onWarning(message);
				}
				else
				{
					throw new InvalidOperationException(message);
				}
			}

			return services;
		}
	}
}
