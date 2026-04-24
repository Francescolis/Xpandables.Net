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
using System.Rests;
using System.Rests.Abstractions;
using System.Rests.RequestBuilders;
using System.Rests.ResponseBuilders;

using Microsoft.Extensions.Http.Resilience;

using Polly;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring and registering services in an IServiceCollection.
/// </summary>
/// <remarks>This class contains static methods that extend the functionality of IServiceCollection, enabling
/// additional service registration patterns and configuration options commonly used in dependency injection
/// scenarios.</remarks>
public static class IRestsExtensions
{
	/// <summary>
	/// Configures resilience policies (retry, circuit breaker, timeout) on the given <see cref="IHttpClientBuilder"/>.
	/// </summary>
	private static IHttpClientBuilder ConfigureResilience(IHttpClientBuilder builder)
	{
		builder.AddResilienceHandler("RestClientResilience", (resilienceBuilder, context) =>
		{
			RestClientOptions options = context.ServiceProvider
				.GetService<Microsoft.Extensions.Options.IOptions<RestClientOptions>>()?.Value
				?? new RestClientOptions();

			// Add retry policy if configured
			if (options.Retry is not null)
			{
				resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
				{
					MaxRetryAttempts = options.Retry.MaxRetryAttempts,
					Delay = options.Retry.Delay,
					MaxDelay = options.Retry.MaxDelay,
					UseJitter = options.Retry.JitterFactor > 0,
					BackoffType = options.Retry.UseExponentialBackoff
						? DelayBackoffType.Exponential
						: DelayBackoffType.Constant,
					ShouldHandle = args =>
					{
						if (args.Outcome.Result is not null)
						{
							int statusCode = (int)args.Outcome.Result.StatusCode;
							bool shouldRetry = options.Retry.RetryableStatusCodes.Contains(statusCode);
							return ValueTask.FromResult(shouldRetry);
						}
						return ValueTask.FromResult(args.Outcome.Exception is not null);
					}
				});
			}

			// Add circuit breaker policy if configured
			if (options.CircuitBreaker is not null)
			{
				resilienceBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
				{
					FailureRatio = options.CircuitBreaker.FailureThreshold / 100.0,
					BreakDuration = options.CircuitBreaker.BreakDuration,
					SamplingDuration = options.CircuitBreaker.SamplingDuration,
					MinimumThroughput = options.CircuitBreaker.MinimumThroughput
				});
			}

			// Add timeout
			resilienceBuilder.AddTimeout(options.Timeout);
		});

		return builder;
	}

	/// <summary>
	/// Adds a singleton implementation of the IRestAttributeProvider service to the service collection.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <typeparam name="TRestAttributeProvider">The type of the RestAttributeProvider to register. Must implement
	/// <see cref="IRestAttributeProvider"/>.</typeparam>
	/// <remarks>Call this method to register the RestAttributeProvider for dependency
	/// injection. This is typically used when configuring services in an application's startup code.</remarks>
	/// <returns>The IServiceCollection instance with the IRestAttributeProvider service registered. This enables further
	/// configuration of the service collection.</returns>
	public static IServiceCollection AddXRestAttributeProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestAttributeProvider>(this IServiceCollection services)
		where TRestAttributeProvider : class, IRestAttributeProvider
		=> services.AddSingleton<IRestAttributeProvider, TRestAttributeProvider>();

	/// <summary>
	/// Adds a singleton implementation of the IRestAttributeProvider service to the service collection.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>Call this method to register the default RestAttributeProvider for dependency
	/// injection. This is typically used when configuring services in an application's startup code.</remarks>
	/// <returns>The IServiceCollection instance with the IRestAttributeProvider service registered. This enables further
	/// configuration of the service collection.</returns>
	public static IServiceCollection AddXRestAttributeProvider(this IServiceCollection services)
		=> services.AddXRestAttributeProvider<RestAttributeProvider>();

	/// <summary>
	/// Adds the default implementation of the RestRequestBuilder service to the dependency injection container.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>Registers RestRequestBuilder with a scoped lifetime. Call this method during
	/// application startup to enable REST request building functionality via dependency injection.</remarks>
	/// <returns>The IServiceCollection instance with the IRestRequestBuilder service registered. This enables further
	/// configuration of the service collection.</returns>
	public static IServiceCollection AddXRestRequestBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestRequestBuilder>(this IServiceCollection services)
		where TRestRequestBuilder : class, IRestRequestBuilder
		=> services.AddSingleton<IRestRequestBuilder, TRestRequestBuilder>();

	/// <summary>
	/// Adds the default implementation of the IRestRequestBuilder service to the dependency injection container.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>Registers RestRequestBuilder with a scoped lifetime. Call this method during
	/// application startup to enable REST request building functionality via dependency injection.</remarks>
	/// <returns>The IServiceCollection instance with the IRestRequestBuilder service registered. This enables further
	/// configuration of the service collection.</returns>
	public static IServiceCollection AddXRestRequestBuilder(this IServiceCollection services) =>
		services.AddXRestRequestBuilder<RestRequestBuilder>();

	/// <summary>
	/// Adds the default implementation of the RestResponseBuilder service to the dependency injection container.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>This method registers RestResponseBuilder as a scoped service for the
	/// IRestResponseBuilder interface. Call this method during application startup to enable response building
	/// functionality for REST APIs.</remarks>
	/// <returns>The IServiceCollection instance with the IRestResponseBuilder service registered.</returns>
	public static IServiceCollection AddXRestResponseBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestResponseBuilder>(this IServiceCollection services)
		where TRestResponseBuilder : class, IRestResponseBuilder
		=> services.AddSingleton<IRestResponseBuilder, TRestResponseBuilder>();

	/// <summary>
	/// Adds the default implementation of the IRestResponseBuilder service to the dependency injection container.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>This method registers RestResponseBuilder as a scoped service for the
	/// IRestResponseBuilder interface. Call this method during application startup to enable response building
	/// functionality for REST APIs.</remarks>
	/// <returns>The IServiceCollection instance with the IRestResponseBuilder service registered.</returns>
	public static IServiceCollection AddXRestResponseBuilder(this IServiceCollection services) =>
		services.AddXRestResponseBuilder<RestResponseBuilder>();

	/// <summary>
	/// Configures REST client options including timeout, retry, circuit breaker, and logging settings.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <param name="configure">A delegate to configure the <see cref="RestClientOptions"/>.</param>
	/// <returns>The <see cref="IServiceCollection"/> instance with the options configured.</returns>
	public static IServiceCollection ConfigureXRestClientOptions(this IServiceCollection services, Action<RestClientOptions> configure)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configure);
		return services.Configure(configure);
	}

	/// <summary>
	/// Adds a request interceptor to the REST client pipeline.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <typeparam name="TInterceptor">The type of the request interceptor to add.</typeparam>
	/// <returns>The <see cref="IServiceCollection"/> instance with the interceptor registered.</returns>
	public static IServiceCollection AddXRestRequestInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInterceptor>(this IServiceCollection services)
		where TInterceptor : class, IRestRequestInterceptor
		=> services.AddTransient<IRestRequestInterceptor, TInterceptor>();

	/// <summary>
	/// Adds a response interceptor to the REST client pipeline.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <typeparam name="TInterceptor">The type of the response interceptor to add.</typeparam>
	/// <returns>The <see cref="IServiceCollection"/> instance with the interceptor registered.</returns>
	public static IServiceCollection AddXRestResponseInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInterceptor>(this IServiceCollection services)
		where TInterceptor : class, IRestResponseInterceptor
		=> services.AddTransient<IRestResponseInterceptor, TInterceptor>();

	/// <summary>
	/// Adds a typed REST client of the specified type to the dependency injection container and configures the
	/// underlying HTTP client.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>This method registers the specified REST client as the implementation for
	/// IRestClient, allowing it to be resolved from the service provider. Use this method to customize the HTTP
	/// client configuration for the REST client, such as setting default request headers or timeouts.</remarks>
	/// <typeparam name="TRestClient">The type of the REST client to register. Must implement the IRestClient interface and have a public
	/// constructor.</typeparam>
	/// <param name="configureClient">A delegate that configures the HttpClient instance for the REST client. Receives the service provider and
	/// the HttpClient to configure.</param>
	/// <returns>An IHttpClientBuilder that can be used to further configure the HTTP client and REST client registration.</returns>
	public static IHttpClientBuilder AddXRestClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClient>(
		this IServiceCollection services,
		Action<IServiceProvider, HttpClient> configureClient)
		where TRestClient : class, IRestClient =>
		services.AddHttpClient<IRestClient, TRestClient>(configureClient);

	/// <summary>
	/// Adds a named REST client of the specified type to the dependency injection container and configures the
	/// underlying HTTP client.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>This method registers the specified REST client as the implementation for
	/// <see cref="IRestClient"/> using a named <see cref="HttpClient"/> registration.
	/// Use this overload when multiple REST clients with different configurations are required.</remarks>
	/// <typeparam name="TRestClient">The type of the REST client to register. Must implement the <see cref="IRestClient"/> interface and have a public
	/// constructor.</typeparam>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <param name="configureClient">A delegate that configures the HttpClient instance for the REST client. Receives the service provider and
	/// the HttpClient to configure.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client and REST client registration.</returns>
	public static IHttpClientBuilder AddXRestClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClient>(
		this IServiceCollection services,
		string name,
		Action<IServiceProvider, HttpClient> configureClient)
		where TRestClient : class, IRestClient =>
		services.AddHttpClient<IRestClient, TRestClient>(name, configureClient);

	/// <summary>
	/// Adds a typed REST client with a custom service type to the dependency injection container and configures the
	/// underlying HTTP client.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>This method registers <typeparamref name="TRestClient"/> as the implementation for
	/// <typeparamref name="TRestClientService"/>, enabling the consumer to resolve a custom service interface
	/// instead of <see cref="IRestClient"/>.</remarks>
	/// <typeparam name="TRestClientService">The service type to register the REST client as. Must implement <see cref="IRestClient"/>.</typeparam>
	/// <typeparam name="TRestClient">The implementation type of the REST client. Must implement <typeparamref name="TRestClientService"/> and have a
	/// public constructor.</typeparam>
	/// <param name="configureClient">A delegate that configures the HttpClient instance for the REST client. Receives the service provider and
	/// the HttpClient to configure.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client and REST client registration.</returns>
	public static IHttpClientBuilder AddXRestClient<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClientService,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClient>(
		this IServiceCollection services,
		Action<IServiceProvider, HttpClient> configureClient)
		where TRestClientService : class, IRestClient
		where TRestClient : class, TRestClientService =>
		services.AddHttpClient<TRestClientService, TRestClient>(configureClient);

	/// <summary>
	/// Adds a named and typed REST client with a custom service type to the dependency injection container and
	/// configures the underlying HTTP client.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>This method combines named and typed <see cref="HttpClient"/> registration, allowing the consumer to
	/// resolve a custom service interface while associating the client with a logical name for configuration
	/// purposes.</remarks>
	/// <typeparam name="TRestClientService">The service type to register the REST client as. Must implement <see cref="IRestClient"/>.</typeparam>
	/// <typeparam name="TRestClient">The implementation type of the REST client. Must implement <typeparamref name="TRestClientService"/> and have a
	/// public constructor.</typeparam>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <param name="configureClient">A delegate that configures the HttpClient instance for the REST client. Receives the service provider and
	/// the HttpClient to configure.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client and REST client registration.</returns>
	public static IHttpClientBuilder AddXRestClient<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClientService,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClient>(
		this IServiceCollection services,
		string name,
		Action<IServiceProvider, HttpClient> configureClient)
		where TRestClientService : class, IRestClient
		where TRestClient : class, TRestClientService =>
		services.AddHttpClient<TRestClientService, TRestClient>(name, configureClient);

	/// <summary>
	/// Adds and configures an XRest HTTP client for dependency injection using the specified client configuration
	/// delegate.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>This method registers the XRest client and its required request and response builders
	/// in the service collection. Use this method to customize the HTTP client settings, such as default headers or
	/// timeouts, before making requests with the XRest client.</remarks>
	/// <param name="configureClient">A delegate that configures the underlying <see cref="HttpClient"/> instance. The delegate receives the
	/// service provider and the <see cref="HttpClient"/> to configure. Cannot be null.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
	public static IHttpClientBuilder AddXRestClient(
		this IServiceCollection services,
		Action<IServiceProvider, HttpClient> configureClient) =>
		services
			.AddXRestRequestBuilder()
			.AddXRestResponseBuilder()
			.AddXRestClient<RestClient>(configureClient);

	/// <summary>
	/// Adds and configures a named XRest HTTP client for dependency injection using the specified client
	/// configuration delegate.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>This method registers the XRest client and its required request and response builders
	/// in the service collection using a named <see cref="HttpClient"/> registration.
	/// Use this overload when multiple REST clients with different configurations are required.</remarks>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <param name="configureClient">A delegate that configures the underlying <see cref="HttpClient"/> instance. The delegate receives the
	/// service provider and the <see cref="HttpClient"/> to configure. Cannot be null.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
	public static IHttpClientBuilder AddXRestClient(
		this IServiceCollection services,
		string name,
		Action<IServiceProvider, HttpClient> configureClient) =>
		services
			.AddXRestRequestBuilder()
			.AddXRestResponseBuilder()
			.AddXRestClient<RestClient>(name, configureClient);

	/// <summary>
	/// Adds and configures an XRest HTTP client with resilience policies (retry and circuit breaker).
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>
	/// This method registers the XRest client with resilience policies based on the configured <see cref="RestClientOptions"/>.
	/// The resilience policies include retry with exponential backoff and circuit breaker patterns.
	/// </remarks>
	/// <param name="configureClient">A delegate that configures the underlying <see cref="HttpClient"/> instance.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
	public static IHttpClientBuilder AddXRestClientWithResilience(
		this IServiceCollection services,
		Action<IServiceProvider, HttpClient> configureClient) =>
		ConfigureResilience(services.AddXRestClient(configureClient));

	/// <summary>
	/// Adds and configures a named XRest HTTP client with resilience policies (retry and circuit breaker).
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>
	/// This method registers the XRest client with a named <see cref="HttpClient"/> and resilience policies
	/// based on the configured <see cref="RestClientOptions"/>.
	/// The resilience policies include retry with exponential backoff and circuit breaker patterns.
	/// </remarks>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <param name="configureClient">A delegate that configures the underlying <see cref="HttpClient"/> instance.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
	public static IHttpClientBuilder AddXRestClientWithResilience(
		this IServiceCollection services,
		string name,
		Action<IServiceProvider, HttpClient> configureClient) =>
		ConfigureResilience(services.AddXRestClient(name, configureClient));

	/// <summary>
	/// Adds and configures a typed XRest HTTP client with resilience policies (retry and circuit breaker).
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>
	/// This method registers the XRest client with a custom service type and resilience policies
	/// based on the configured <see cref="RestClientOptions"/>.
	/// The resilience policies include retry with exponential backoff and circuit breaker patterns.
	/// The default request and response builders are registered automatically.
	/// </remarks>
	/// <typeparam name="TRestClientService">The service type to register the REST client as. Must implement <see cref="IRestClient"/>.</typeparam>
	/// <typeparam name="TRestClient">The implementation type of the REST client. Must implement <typeparamref name="TRestClientService"/> and have a
	/// public constructor.</typeparam>
	/// <param name="configureClient">A delegate that configures the underlying <see cref="HttpClient"/> instance.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
	public static IHttpClientBuilder AddXRestClientWithResilience<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClientService,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClient>(
		this IServiceCollection services,
		Action<IServiceProvider, HttpClient> configureClient)
		where TRestClientService : class, IRestClient
		where TRestClient : class, TRestClientService =>
		ConfigureResilience(
			services
				.AddXRestRequestBuilder()
				.AddXRestResponseBuilder()
				.AddXRestClient<TRestClientService, TRestClient>(configureClient));

	/// <summary>
	/// Adds and configures a named and typed XRest HTTP client with resilience policies (retry and circuit breaker).
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>
	/// This method registers the XRest client with a named <see cref="HttpClient"/> and a custom service type,
	/// along with resilience policies based on the configured <see cref="RestClientOptions"/>.
	/// The resilience policies include retry with exponential backoff and circuit breaker patterns.
	/// The default request and response builders are registered automatically.
	/// </remarks>
	/// <typeparam name="TRestClientService">The service type to register the REST client as. Must implement <see cref="IRestClient"/>.</typeparam>
	/// <typeparam name="TRestClient">The implementation type of the REST client. Must implement <typeparamref name="TRestClientService"/> and have a
	/// public constructor.</typeparam>
	/// <param name="name">The logical name of the <see cref="HttpClient"/> to configure.</param>
	/// <param name="configureClient">A delegate that configures the underlying <see cref="HttpClient"/> instance.</param>
	/// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
	public static IHttpClientBuilder AddXRestClientWithResilience<
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClientService,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClient>(
		this IServiceCollection services,
		string name,
		Action<IServiceProvider, HttpClient> configureClient)
		where TRestClientService : class, IRestClient
		where TRestClient : class, TRestClientService =>
		ConfigureResilience(
			services
				.AddXRestRequestBuilder()
				.AddXRestResponseBuilder()
				.AddXRestClient<TRestClientService, TRestClient>(name, configureClient));

	/// <summary>
	/// Registers the default set of REST request composer implementations with the dependency injection container.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>Call this method to add support for composing various types of REST requests, such as
	/// headers, cookies, authentication, and multipart content. This method is intended to be used during
	/// application startup as part of service registration.</remarks>
	/// <returns>The <see cref="IServiceCollection"/> instance with the REST request composer services registered. This
	/// enables further configuration of the service collection.</returns>
	public static IServiceCollection AddXRestRequestComposers(this IServiceCollection services) =>
		services
			.AddTransient<IRestRequestComposer, RestPatchComposer>()
			.AddTransient<IRestRequestComposer, RestQueryStringComposer>()
			.AddTransient<IRestRequestComposer, RestCookieComposer>()
			.AddTransient<IRestRequestComposer, RestHeaderComposer>()
			.AddTransient<IRestRequestComposer, RestBasicAuthComposer>()
			.AddTransient<IRestRequestComposer, RestByteArrayComposer>()
			.AddTransient<IRestRequestComposer, RestFormUrlEncodedComposer>()
			.AddTransient<IRestRequestComposer, RestMultipartComposer>()
			.AddTransient<IRestRequestComposer, RestStreamComposer>()
			.AddTransient<IRestRequestComposer, RestPathStringComposer>()
			.AddTransient<IRestRequestComposer, RestStringComposer>();

	/// <summary>
	/// Registers the default REST response composer services required for XRest in the dependency injection
	/// container.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> instance to extend.</param>
	/// <remarks>Call this method during application startup to ensure that all necessary response
	/// composers are available for XRest operations. This method should typically be called once per
	/// application.</remarks>
	/// <returns>The <see cref="IServiceCollection"/> instance with the XRest response composer services added. This enables
	/// further configuration via method chaining.</returns>
	public static IServiceCollection AddXRestResponseComposers(this IServiceCollection services) =>
		services
			.AddTransient<IRestResponseComposer, RestResponseFailureComposer>()
			.AddTransient<IRestResponseComposer, RestResponseContentComposer>()
			.AddTransient<IRestResponseComposer, RestResponseNoContentComposer>()
			.AddTransient<IRestResponseComposer, RestResponseResultComposer>()
			.AddTransient<IRestResponseComposer, RestResponseStreamComposer>()
			.AddTransient<IRestResponseComposer, RestResponseStreamPagedComposer>();
}
