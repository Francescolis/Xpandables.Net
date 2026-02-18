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
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering and configuring minimal endpoint routes and related metadata in ASP.NET
/// Core applications.
/// </summary>
/// <remarks>This static class contains helper methods for adding minimal support services, discovering and
/// registering endpoint routes, and configuring endpoint metadata such as accepted request types and response status
/// codes. Use these methods during application startup to streamline the setup of minimal APIs and ensure consistent
/// endpoint metadata across your application.</remarks>
public static class IMinimalEndpointRouteExtensions
{
	/// <summary>
	/// Adds minimal support services and configuration options to the specified service collection.
	/// </summary>
	/// <remarks>This method registers a singleton instance of the configured minimal support options. Call
	/// this method during application startup to enable minimal support features.</remarks>
	/// <param name="services">The service collection to which the minimal support services and options will be added. Cannot be null.</param>
	/// <param name="configure">A delegate used to configure the minimal support options before they are registered.</param>
	/// <returns>The same service collection instance with minimal support services and options registered.</returns>
	public static IServiceCollection AddXMinimalSupport(
		this IServiceCollection services,
		Action<MinimalSupportOptions>? configure = default)
	{
		ArgumentNullException.ThrowIfNull(services);

		var options = new MinimalSupportOptions();
		configure?.Invoke(options);

		services.AddSingleton(options);
		return services;
	}

	/// <summary>
	/// Registers all sealed classes implementing <see cref="IMinimalEndpointRoute"/> from the specified assemblies as
	/// transient services in the dependency injection container.
	/// </summary>
	/// <remarks>Each discovered sealed class implementing <see cref="IMinimalEndpointRoute"/> is registered
	/// as a transient service. This enables automatic discovery and registration of minimal endpoint routes for use in
	/// ASP.NET Core applications.</remarks>
	/// <param name="services">The <see cref="IServiceCollection"/> to which the endpoint route services will be added. Cannot be null.</param>
	/// <param name="assemblies">An array of assemblies to scan for sealed classes implementing <see cref="IMinimalEndpointRoute"/>. If not
	/// specified or empty, the calling assembly is used.</param>
	/// <returns>The <see cref="IServiceCollection"/> instance with the endpoint route services registered.</returns>
	[RequiresUnreferencedCode("This method may be trimmed.")]
	public static IServiceCollection AddXMinimalEndpointRoutes(
		this IServiceCollection services,
		params IEnumerable<Assembly> assemblies)
	{
		ArgumentNullException.ThrowIfNull(services);

		Assembly[] assembliesArray = assemblies as Assembly[] ?? [.. assemblies];
		assembliesArray = assembliesArray is { Length: > 0 } ? assembliesArray : [Assembly.GetCallingAssembly()];

		List<Type> endpointTypes = [.. assembliesArray
			.SelectMany(assembly => assembly.GetTypes())
			.Where(type => type is
			{
				IsInterface: false,
				IsGenericType: false,
				IsClass: true,
				IsSealed: true
			}
				&& typeof(IMinimalEndpointRoute).IsAssignableFrom(type))];

		foreach (var endpointType in endpointTypes)
		{
			services.Add(new ServiceDescriptor(
				typeof(IMinimalEndpointRoute),
				endpointType,
				ServiceLifetime.Transient));
		}

		return services;
	}

	/// <summary>
	/// Configures the application to use all registered minimal endpoint routes.
	/// </summary>
	/// <remarks>This method discovers all services implementing <see cref="IMinimalEndpointRoute"/> and adds
	/// their routes to the application using the provided <see cref="MinimalSupportOptions"/>. Call this method during
	/// application startup to ensure all minimal endpoints are registered before the app runs.</remarks>
	/// <param name="application">The <see cref="WebApplication"/> instance to configure. Cannot be null.</param>
	/// <returns>The same <see cref="WebApplication"/> instance, enabling method chaining.</returns>
	public static WebApplication UseXMinimalEndpointRoutes(this WebApplication application)
	{
		ArgumentNullException.ThrowIfNull(application);

		MinimalSupportOptions options = application.Services.GetRequiredService<MinimalSupportOptions>();
		IEnumerable<IMinimalEndpointRoute> endpointRoutes = application.Services
			.GetServices<IMinimalEndpointRoute>();

		foreach (var route in endpointRoutes)
		{
			route.AddRoutes(new MinimalRouteBuilder(application, options));
		}

		return application;
	}

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IAcceptsMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the 
	/// route builder.
	/// </summary>
	/// <param name="builder">The route builder.</param>
	/// <typeparam name="TRequest">The type of the request.</typeparam>
	/// <returns>The route builder.</returns>
	public static RouteHandlerBuilder Accepts<TRequest>(
		this RouteHandlerBuilder builder)
		where TRequest : notnull =>
		builder.Accepts<TRequest>("application/json");

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
	/// route builder.
	/// </summary>
	/// <param name="builder">The route builder.</param>
	/// <returns>The route builder.</returns>
	public static TBuilder Produces200OK<TBuilder>(
		this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.WithMetadata(
			new ProducesResponseTypeMetadata(
				StatusCodes.Status200OK,
				typeof(void),
				["application/json"]));

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
	/// route builder.
	/// </summary>
	/// <param name="builder">The route builder.</param>
	/// <typeparam name="TResponse">The type of the response.</typeparam>
	/// <returns>The route builder.</returns>
	public static RouteHandlerBuilder Produces200OK<TResponse>(
		this RouteHandlerBuilder builder)
		where TResponse : notnull =>
		builder.Produces<TResponse>(
			StatusCodes.Status200OK,
			"application/json");

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
	/// route builder.
	/// </summary>
	/// <param name="builder">The route builder.</param>
	/// <typeparam name="TResponse">The type of the response.</typeparam>
	/// <returns>The route builder.</returns>
	public static RouteHandlerBuilder Produces201Created<TResponse>(
		this RouteHandlerBuilder builder)
		where TResponse : notnull =>
		builder.Produces<TResponse>(
			StatusCodes.Status201Created,
			"application/json");

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
	/// route builder.
	/// </summary>
	/// <param name="builder">The route builder.</param>
	/// <returns>The route builder.</returns>
	public static TBuilder Produces400BadRequest<TBuilder>(
		this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.ProducesValidationProblem(
			StatusCodes.Status400BadRequest,
			"application/problem+json");

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
	/// route builder.
	/// </summary>
	/// <param name="builder">The route builder.</param>
	/// <returns>The route builder.</returns>
	public static TBuilder Produces404NotFound<TBuilder>(
		this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.ProducesValidationProblem(
			StatusCodes.Status404NotFound,
			"application/problem+json");

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
	/// route builder.
	/// </summary>
	/// <param name="builder">The route builder.</param>
	/// <returns>The route builder.</returns>
	public static TBuilder Produces409Conflict<TBuilder>(
		this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.ProducesValidationProblem(
			StatusCodes.Status409Conflict,
			"application/problem+json");

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
	/// route builder.
	/// </summary>
	/// <param name="builder">The route builder.</param>
	/// <returns>The route builder.</returns>
	public static TBuilder Produces405MethodNotAllowed<TBuilder>(
		this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.ProducesValidationProblem(
			StatusCodes.Status405MethodNotAllowed,
			"application/problem+json");

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
	/// route builder.
	/// </summary>
	/// <param name="builder">The route builder.</param>
	/// <returns>The route builder.</returns>
	public static TBuilder Produces401Unauthorized<TBuilder>(
		this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.ProducesValidationProblem(
			StatusCodes.Status401Unauthorized,
			"application/problem+json");

	/// <summary>
	/// Adds <see cref="Microsoft.AspNetCore.Http.Metadata.IProducesResponseTypeMetadata"/>
	/// to the <see cref="Endpoint.Metadata"/> for all endpoints produced by the
	/// route builder.
	/// </summary>
	/// <typeparam name="TBuilder">The type of the route builder.</typeparam>
	/// <param name="builder">The route builder.</param>
	/// <returns>The route builder.</returns>
	public static TBuilder Produces500InternalServerError<TBuilder>(
		this TBuilder builder)
		where TBuilder : IEndpointConventionBuilder =>
		builder.ProducesProblem(
			StatusCodes.Status500InternalServerError,
			"application/problem+json");
}
