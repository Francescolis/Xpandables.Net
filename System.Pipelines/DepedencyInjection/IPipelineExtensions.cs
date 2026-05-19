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
using System.Pipelines;

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
	/// Adds the default pipeline request handler implementation to the service collection for dependency injection.
	/// </summary>
	/// <returns>The updated <see cref="IServiceCollection"/> instance with the pipeline request handler registered.</returns>
	public static IServiceCollection AddXPipelineRequestHandler(this IServiceCollection services)
	{
		return services.AddXPipelineRequestHandler(typeof(PipelineRequestHandler<>));
	}

	/// <summary>
	/// Registers a pipeline request handler of the specified type with the dependency injection container.
	/// </summary>
	/// <param name="services">The IServiceCollection instance to which the cache type resolver will be added.</param>
	/// <remarks>Use this method to add custom pipeline request handlers to the service collection for
	/// dependency injection. The handler will be registered with scoped lifetime to amortize the cost of
	/// pipeline construction and to align with scoped decorator dependencies.</remarks>
	/// <param name="type">The type that implements the <see cref="IPipelineRequestHandler{TRequest}"/> interface. Must have public constructors and
	/// implement the required interface.</param>
	/// <returns>The IServiceCollection instance with the pipeline request handler registered as a scoped service.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the specified type does not implement the <see cref="IPipelineRequestHandler{TRequest}"/>> interface.</exception>
	public static IServiceCollection AddXPipelineRequestHandler(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
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
	/// Registers the specified pipeline decorator type as a transient implementation of the <see cref="IPipelineDecorator{TRequest}"/>
	/// interface in the service collection.
	/// </summary>
	/// <param name="services">The IServiceCollection instance to which the cache type resolver will be added.</param>
	/// <remarks>Use this method to add custom pipeline decorators to the dependency injection
	/// container. The decorator type must implement the <see cref="IPipelineDecorator{TRequest}"/> interface to be registered
	/// successfully.</remarks>
	/// <param name="pipeline">The type of the pipeline decorator to register. Must implement the <see cref="IPipelineDecorator{TRequest}"/> interface and have
	/// public constructors.</param>
	/// <returns>The updated IServiceCollection instance with the pipeline decorator registered.</returns>
	/// <exception cref="InvalidOperationException">Thrown if pipeline does not implement the <see cref="IPipelineDecorator{TRequest}"/> interface.</exception>
	public static IServiceCollection AddXPipelineDecorator(
		this IServiceCollection services,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] Type pipeline)
	{
		ArgumentNullException.ThrowIfNull(pipeline);
		ArgumentNullException.ThrowIfNull(services);

		if (!pipeline.GetInterfaces().Any(i =>
				i.IsGenericType
				&& i.GetGenericTypeDefinition() == typeof(IPipelineDecorator<>)))
		{
			throw new InvalidOperationException(
				$"{pipeline.Name} does not implement IPipelineDecorator<> interface.");
		}

		return services.AddScoped(typeof(IPipelineDecorator<>), pipeline);
	}
}
