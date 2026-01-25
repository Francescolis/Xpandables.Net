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
    /// Provides extensions methods for <see cref="IServiceCollection"/>.
    /// </summary>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds a singleton implementation of the IRestAttributeProvider service to the service collection.
        /// </summary>
        /// <typeparam name="TRestAttributeProvider">The type of the RestAttributeProvider to register. Must implement
        /// <see cref="IRestAttributeProvider"/>.</typeparam>
        /// <remarks>Call this method to register the RestAttributeProvider for dependency
        /// injection. This is typically used when configuring services in an application's startup code.</remarks>
        /// <returns>The IServiceCollection instance with the IRestAttributeProvider service registered. This enables further
        /// configuration of the service collection.</returns>
        public IServiceCollection AddXRestAttributeProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestAttributeProvider>()
            where TRestAttributeProvider : class, IRestAttributeProvider
            => services.AddSingleton<IRestAttributeProvider, TRestAttributeProvider>();

        /// <summary>
        /// Adds a singleton implementation of the IRestAttributeProvider service to the service collection.
        /// </summary>
        /// <remarks>Call this method to register the default RestAttributeProvider for dependency
        /// injection. This is typically used when configuring services in an application's startup code.</remarks>
        /// <returns>The IServiceCollection instance with the IRestAttributeProvider service registered. This enables further
        /// configuration of the service collection.</returns>
        public IServiceCollection AddXRestAttributeProvider()
            => services.AddXRestAttributeProvider<RestAttributeProvider>();

        /// <summary>
        /// Adds the default implementation of the RestRequestBuilder service to the dependency injection container.
        /// </summary>
        /// <remarks>Registers RestRequestBuilder with a scoped lifetime. Call this method during
        /// application startup to enable REST request building functionality via dependency injection.</remarks>
        /// <returns>The IServiceCollection instance with the IRestRequestBuilder service registered. This enables further
        /// configuration of the service collection.</returns>
        public IServiceCollection AddXRestRequestBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestRequestBuilder>()
            where TRestRequestBuilder : class, IRestRequestBuilder
            => services.AddScoped<IRestRequestBuilder, TRestRequestBuilder>();

        /// <summary>
        /// Adds the default implementation of the IRestRequestBuilder service to the dependency injection container.
        /// </summary>
        /// <remarks>Registers RestRequestBuilder with a scoped lifetime. Call this method during
        /// application startup to enable REST request building functionality via dependency injection.</remarks>
        /// <returns>The IServiceCollection instance with the IRestRequestBuilder service registered. This enables further
        /// configuration of the service collection.</returns>
        public IServiceCollection AddXRestRequestBuilder() =>
            services.AddXRestRequestBuilder<RestRequestBuilder>();

        /// <summary>
        /// Adds the default implementation of the RestResponseBuilder service to the dependency injection container.
        /// </summary>
        /// <remarks>This method registers RestResponseBuilder as a scoped service for the
        /// IRestResponseBuilder interface. Call this method during application startup to enable response building
        /// functionality for REST APIs.</remarks>
        /// <returns>The IServiceCollection instance with the IRestResponseBuilder service registered.</returns>
        public IServiceCollection AddXRestResponseBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestResponseBuilder>()
            where TRestResponseBuilder : class, IRestResponseBuilder
            => services.AddScoped<IRestResponseBuilder, TRestResponseBuilder>();

        /// <summary>
        /// Adds the default implementation of the IRestResponseBuilder service to the dependency injection container.
        /// </summary>
        /// <remarks>This method registers RestResponseBuilder as a scoped service for the
        /// IRestResponseBuilder interface. Call this method during application startup to enable response building
        /// functionality for REST APIs.</remarks>
        /// <returns>The IServiceCollection instance with the IRestResponseBuilder service registered.</returns>
        public IServiceCollection AddXRestResponseBuilder() =>
            services.AddXRestResponseBuilder<RestResponseBuilder>();

        /// <summary>
        /// Configures REST client options including timeout, retry, circuit breaker, and logging settings.
        /// </summary>
        /// <param name="configure">A delegate to configure the <see cref="RestClientOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance with the options configured.</returns>
        public IServiceCollection ConfigureXRestClientOptions(Action<RestClientOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(configure);
            return services.Configure(configure);
        }

        /// <summary>
        /// Adds a request interceptor to the REST client pipeline.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the request interceptor to add.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the interceptor registered.</returns>
        public IServiceCollection AddXRestRequestInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInterceptor>()
            where TInterceptor : class, IRestRequestInterceptor
            => services.AddTransient<IRestRequestInterceptor, TInterceptor>();

        /// <summary>
        /// Adds a response interceptor to the REST client pipeline.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the response interceptor to add.</typeparam>
        /// <returns>The <see cref="IServiceCollection"/> instance with the interceptor registered.</returns>
        public IServiceCollection AddXRestResponseInterceptor<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TInterceptor>()
            where TInterceptor : class, IRestResponseInterceptor
            => services.AddTransient<IRestResponseInterceptor, TInterceptor>();

        /// <summary>
        /// Adds a typed REST client of the specified type to the dependency injection container and configures the
        /// underlying HTTP client.
        /// </summary>
        /// <remarks>This method registers the specified REST client as the implementation for
        /// IRestClient, allowing it to be resolved from the service provider. Use this method to customize the HTTP
        /// client configuration for the REST client, such as setting default request headers or timeouts.</remarks>
        /// <typeparam name="TRestClient">The type of the REST client to register. Must implement the IRestClient interface and have a public
        /// constructor.</typeparam>
        /// <param name="configureClient">A delegate that configures the HttpClient instance for the REST client. Receives the service provider and
        /// the HttpClient to configure.</param>
        /// <returns>An IHttpClientBuilder that can be used to further configure the HTTP client and REST client registration.</returns>
        public IHttpClientBuilder AddXRestClient<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRestClient>(
            Action<IServiceProvider, HttpClient> configureClient)
            where TRestClient : class, IRestClient =>
            services.AddHttpClient<IRestClient, TRestClient>(configureClient);

        /// <summary>
        /// Adds and configures an XRest HTTP client for dependency injection using the specified client configuration
        /// delegate.
        /// </summary>
        /// <remarks>This method registers the XRest client and its required request and response builders
        /// in the service collection. Use this method to customize the HTTP client settings, such as default headers or
        /// timeouts, before making requests with the XRest client.</remarks>
        /// <param name="configureClient">A delegate that configures the underlying <see cref="HttpClient"/> instance. The delegate receives the
        /// service provider and the <see cref="HttpClient"/> to configure. Cannot be null.</param>
        /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
        public IHttpClientBuilder AddXRestClient(
            Action<IServiceProvider, HttpClient> configureClient) =>
            services
                .AddXRestRequestBuilder()
                .AddXRestResponseBuilder()
                .AddXRestClient<RestClient>(configureClient);

        /// <summary>
        /// Adds and configures an XRest HTTP client with resilience policies (retry and circuit breaker).
        /// </summary>
        /// <remarks>
        /// This method registers the XRest client with resilience policies based on the configured <see cref="RestClientOptions"/>.
        /// The resilience policies include retry with exponential backoff and circuit breaker patterns.
        /// </remarks>
        /// <param name="configureClient">A delegate that configures the underlying <see cref="HttpClient"/> instance.</param>
        /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to further configure the HTTP client.</returns>
        public IHttpClientBuilder AddXRestClientWithResilience(
            Action<IServiceProvider, HttpClient> configureClient)
        {
            var builder = services.AddXRestClient(configureClient);

            builder.AddResilienceHandler("RestClientResilience", (resilienceBuilder, context) =>
            {
                var options = context.ServiceProvider
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
        /// Registers the default set of REST request composer implementations with the dependency injection container.
        /// </summary>
        /// <remarks>Call this method to add support for composing various types of REST requests, such as
        /// headers, cookies, authentication, and multipart content. This method is intended to be used during
        /// application startup as part of service registration.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the REST request composer services registered. This
        /// enables further configuration of the service collection.</returns>
        public IServiceCollection AddXRestRequestComposers() =>
            services
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestPatchComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestQueryStringComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestCookieComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestHeaderComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestBasicAuthComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestByteArrayComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestFormUrlEncodedComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestMultipartComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestStreamComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestPathStringComposer<>))
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestStringComposer<>));

        /// <summary>
        /// Registers the default REST response composer services required for XRest in the dependency injection
        /// container.
        /// </summary>
        /// <remarks>Call this method during application startup to ensure that all necessary response
        /// composers are available for XRest operations. This method should typically be called once per
        /// application.</remarks>
        /// <returns>The <see cref="IServiceCollection"/> instance with the XRest response composer services added. This enables
        /// further configuration via method chaining.</returns>
        public IServiceCollection AddXRestResponseComposers() =>
            services
                .AddTransient<IRestResponseComposer, RestResponseFailureComposer>()
                .AddTransient<IRestResponseComposer, RestResponseContentComposer>()
                .AddTransient<IRestResponseComposer, RestResponseNoContentComposer>()
                .AddTransient<IRestResponseComposer, RestResponseResultComposer>()
                .AddTransient<IRestResponseComposer, RestResponseStreamComposer>()
                .AddTransient<IRestResponseComposer, RestResponseStreamPagedComposer>();
    }
}