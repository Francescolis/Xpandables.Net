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
using System.Reflection;
using System.Results.Pipelines;
using System.Results.Requests;

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
public static class IResultExtensions
{
	internal readonly record struct HandlerType(Type Type, IEnumerable<Type> Interfaces);

	private static readonly HashSet<Type> HandlerInterfaceDefinitions =
	[
		typeof(IRequestHandler<>),
		typeof(IRequestHandler<,>),
		typeof(IRequestContextHandler<>),
		typeof(IRequestContextHandler<,>),
		typeof(IRequestPostHandler<>),
		typeof(IStreamRequestHandler<,>),
		typeof(IStreamPagedRequestHandler<,>),
		typeof(IStreamRequestContextHandler<,>),
		typeof(IStreamPagedRequestContextHandler<,>),
		typeof(IRequestPreHandler<>)
	];

	private static bool IsHandlerInterface(Type i) =>
		i.IsGenericType && HandlerInterfaceDefinitions.Contains(i.GetGenericTypeDefinition());

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
	public static IServiceCollection AddXPipelineDecorator(this IServiceCollection services, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] Type pipeline)
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

	/// <summary>
	/// Registers all sealed request handler types from the specified assemblies with the dependency injection container as
	/// transient services.
	/// </summary>
	/// <param name="services">The IServiceCollection instance to which the cache type resolver will be added.</param>
	/// <remarks>Handler types are identified by their implementation of supported handler interfaces,
	/// such as <see cref="IRequestHandler{TRequest}"/>, <see cref="IRequestContextHandler{TRequest}"/>, and related
	/// interfaces. Only sealed, non-abstract classes are registered. Open generic implementations
	/// (e.g., <c>MyHandler&lt;TRequest&gt; : IRequestHandler&lt;TRequest&gt;</c>) are registered as open generic
	/// services, allowing the DI container to resolve them for any concrete request type.
	/// Each handler interface is registered as a scoped service. This method requires dynamic code generation
	/// and may require unreferenced code; see the method attributes for details.</remarks>
	/// <param name="assemblies">An array of assemblies to scan for handler implementations. If not specified or empty, the calling assembly
	/// is used.</param>
	/// <returns>The <see cref="IServiceCollection"/> instance with handler services registered.</returns>
	[RequiresDynamicCode("Dynamic code generation is required for this method.")]
	[RequiresUnreferencedCode("Calls MakeGenericMethod which may require unreferenced code.")]
	public static IServiceCollection AddXRequestHandlers(this IServiceCollection services, params IEnumerable<Assembly> assemblies)
	{
		ArgumentNullException.ThrowIfNull(services);

		Assembly[] assembliesArray = assemblies as Assembly[] ?? [.. assemblies];
		assembliesArray = assembliesArray is { Length: > 0 } ? assembliesArray : [Assembly.GetCallingAssembly()];

		IEnumerable<Type> handlerTypes = assembliesArray
			.SelectMany(assembly => assembly.GetTypes())
			.Where(type =>
				type is { IsClass: true, IsAbstract: false, IsSealed: true }
				&& type.GetInterfaces().Any(IsHandlerInterface));

		foreach (Type type in handlerTypes)
		{
			IEnumerable<Type> interfaceTypes = type.GetInterfaces().Where(IsHandlerInterface);

			if (type.IsGenericTypeDefinition)
			{
				foreach (Type interfaceType in interfaceTypes)
				{
					services.AddScoped(interfaceType.GetGenericTypeDefinition(), type);
				}

				continue;
			}

			foreach (Type interfaceType in interfaceTypes)
			{
				services.AddScoped(interfaceType, type);
			}
		}

		return services;
	}
}
