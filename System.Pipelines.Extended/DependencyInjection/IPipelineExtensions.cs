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
using System.Pipelines;

namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering pipeline decorators with an <see cref="IServiceCollection"/>.
/// </summary>
public static class IPipelineExtensions
{
	/// <summary>
	/// Adds the default post-processing decorator to the pipeline configuration for all registered pipeline
	/// handlers.
	/// </summary>
	/// <remarks>This method registers <see cref="PipelinePostHandlerDecorator{TRequest}"/> as a decorator
	/// for all pipeline handler types. Call this method during service configuration to enable post-processing
	/// behavior in the pipeline.</remarks>
	/// <returns>An <see cref="IServiceCollection"/> containing the service registrations, including the post-processing
	/// decorator.</returns>
	public static IServiceCollection AddXPipelinePostHanderDecorator(this IServiceCollection services) =>
		services.AddXPipelineDecorator(typeof(PipelinePostHandlerDecorator<>));

	/// <summary>
	/// Registers the default pre-decorator for Pipeline into the service collection.
	/// </summary>
	/// <remarks>This method adds the generic <c>PipelinePreDecorator&lt;T&gt;</c> to the service
	/// collection, enabling pre-processing behavior for Pipeline handlers. Call this method during application
	/// startup to configure the pipeline decorators before resolving pipeline services.</remarks>
	/// <returns>The updated <see cref="IServiceCollection"/> instance with the XPipeline pre-decorator registered.</returns>
	public static IServiceCollection AddXPipelinePreHanderDecorator(this IServiceCollection services) =>
		services.AddXPipelineDecorator(typeof(PipelinePreHandlerDecorator<>));

	/// <summary>
	/// Registers the <see cref="PipelineCachingDecorator{TRequest}"/> for caching successful results
	/// of requests implementing <see cref="IRequiresCacheable"/>.
	/// </summary>
	/// <remarks>Register this decorator after validation but before the handler to avoid
	/// redundant processing. An <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/> must
	/// be registered in the container (e.g., via <c>AddMemoryCache()</c>).</remarks>
	/// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
	public static IServiceCollection AddXPipelineCachingDecorator(this IServiceCollection services) =>
		services.AddXPipelineDecorator(typeof(PipelineCachingDecorator<>));

	/// <summary>
	/// Adds the PipelineExceptionDecorator to the pipeline, enabling exception handling for pipeline operations.
	/// </summary>
	/// <remarks>This method registers the PipelineExceptionDecorator for all pipeline types. Use this
	/// to ensure that exceptions thrown during pipeline execution are handled consistently across the
	/// application.</remarks>
	/// <returns>The same IServiceCollection instance, allowing for method chaining.</returns>
	public static IServiceCollection AddXPipelineExceptionDecorator(this IServiceCollection services) =>
		services.AddXPipelineDecorator(typeof(PipelineExceptionDecorator<>));

	/// <summary>
	/// Validates that the pipeline decorator registrations include the exception decorator.
	/// </summary>
	/// <param name="services">The IServiceCollection instance to which the cache type resolver will be added.</param>
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
	public static IServiceCollection ValidateXPipelineExceptionRegistration(this IServiceCollection services, Action<string>? onWarning = null)
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
