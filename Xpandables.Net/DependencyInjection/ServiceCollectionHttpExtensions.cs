
/************************************************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
************************************************************************************************************/
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xpandables.Net.Http;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register Http services.
/// </summary>
public static class ServiceCollectionHttpExtensions
{
    /// <summary>
    /// Registers the <see cref="IHttpClientBuildProvider"/> default implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientBuildProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IHttpClientBuildProvider, HttpClientBuildProviderInternal>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="IHttpClientRequestBuilder"/> default implementation to 
    /// the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientRequestBuilder(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IHttpClientRequestBuilder, HttpClientRequestBuilderInternal>();
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="THttpClientRequestBuilder"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="THttpClientRequestBuilder">The type to be used to build the request.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientRequestBuilder<THttpClientRequestBuilder>(this IServiceCollection services)
        where THttpClientRequestBuilder : class, IHttpClientRequestBuilder
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IHttpClientRequestBuilder, THttpClientRequestBuilder>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="IHttpClientResponseBuilder"/> default implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientResponseBuilder(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IHttpClientResponseBuilder, HttpClientResponseBuilderInternal>();
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="THttpClientResponseBuilder"/> type implementation 
    /// to the services with scope life time.
    /// </summary>
    /// <typeparam name="THttpClientResponseBuilder">The type to use to build the response.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientResponseBuilder<THttpClientResponseBuilder>(this IServiceCollection services)
        where THttpClientResponseBuilder : class, IHttpClientResponseBuilder
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IHttpClientResponseBuilder, THttpClientResponseBuilder>();
        return services;
    }

    /// <summary>
    ///  Registers the default implementation of <see cref="IHttpClientDispatcher"/> type and configures a 
    ///  binding with a named <see cref="HttpClient"/>. 
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="configureClient"/> is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHttpClient<IHttpClientDispatcher, DefaultHttpClientDispatcher>(configureClient);
    }

    /// <summary>
    ///  Registers the default implementation of <see cref="IHttpClientDispatcher"/> type and configures a 
    ///  binding with a named <see cref="HttpClient"/> and security header value provider for authorization. 
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="authenticationHeaderValue">The security header value provider.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="configureClient"/> is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher(
        this IServiceCollection services,
        HttpClientAuthenticationHeaderValueProvider authenticationHeaderValue,
        Action<IServiceProvider, HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped<HttpClientAuthorizationHandler>()
            .AddScoped(_ => authenticationHeaderValue)
            .AddHttpClient<IHttpClientDispatcher, DefaultHttpClientDispatcher>(configureClient)
            .ConfigurePrimaryHttpMessageHandler<HttpClientAuthorizationHandler>();
    }

    /// <summary>
    /// Registers the <typeparamref name="THttpClientDispatcherImpl"/> type as 
    /// <typeparamref name="THttpClientDispatcherInterface"/>
    /// to be used as <see cref="IHttpClientDispatcher"/>.
    /// </summary>
    /// <typeparam name="THttpClientDispatcherInterface">The interface type 
    /// that inherits from <see cref="IHttpClientDispatcher"/>.</typeparam>
    /// <typeparam name="THttpClientDispatcherImpl">The type that implements 
    /// <typeparamref name="THttpClientDispatcherInterface"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher<THttpClientDispatcherInterface, THttpClientDispatcherImpl>(
        this IServiceCollection services)
        where THttpClientDispatcherInterface : class, IHttpClientDispatcher
        where THttpClientDispatcherImpl : class, THttpClientDispatcherInterface
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHttpClient<THttpClientDispatcherInterface, THttpClientDispatcherImpl>();
    }

    /// <summary>
    /// Registers the <typeparamref name="THttpClientDispatcherImpl"/> type as 
    /// <typeparamref name="THttpClientDispatcherInterface"/>
    /// to be used as <see cref="IHttpClientDispatcher"/>.
    /// </summary>
    /// <typeparam name="THttpClientDispatcherInterface">The interface type 
    /// that inherits from <see cref="IHttpClientDispatcher"/>.</typeparam>
    /// <typeparam name="THttpClientDispatcherImpl">The type that implements 
    /// <typeparamref name="THttpClientDispatcherInterface"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">A delegate that is used to configure 
    /// an <see cref="HttpClient"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher<THttpClientDispatcherInterface, THttpClientDispatcherImpl>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where THttpClientDispatcherInterface : class, IHttpClientDispatcher
        where THttpClientDispatcherImpl : class, THttpClientDispatcherInterface
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHttpClient<THttpClientDispatcherInterface, THttpClientDispatcherImpl>(configureClient);
    }

    /// <summary>
    /// Registers the <typeparamref name="THttpClientDispatcherImpl"/> type as 
    /// <typeparamref name="THttpClientDispatcherInterface"/>
    /// to be used as <see cref="IHttpClientDispatcher"/> and a security header value provider for authorization.
    /// </summary>
    /// <typeparam name="THttpClientDispatcherInterface">The interface type that 
    /// inherits from <see cref="IHttpClientDispatcher"/>.</typeparam>
    /// <typeparam name="THttpClientDispatcherImpl">The type that implements 
    /// <typeparamref name="THttpClientDispatcherInterface"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="authenticationHeaderValue">The security header value provider.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher<THttpClientDispatcherInterface, THttpClientDispatcherImpl>(
        this IServiceCollection services,
        HttpClientAuthenticationHeaderValueProvider authenticationHeaderValue)
        where THttpClientDispatcherInterface : class, IHttpClientDispatcher
        where THttpClientDispatcherImpl : class, THttpClientDispatcherInterface
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .AddScoped<HttpClientAuthorizationHandler>()
            .AddScoped(_ => authenticationHeaderValue)
            .AddHttpClient<THttpClientDispatcherInterface, THttpClientDispatcherImpl>()
            .ConfigurePrimaryHttpMessageHandler<HttpClientAuthorizationHandler>();
    }

    /// <summary>
    /// Registers the <typeparamref name="THttpClientDispatcherImpl"/> type
    /// as <typeparamref name="THttpClientDispatcherInterface"/>
    /// to be used as <see cref="IHttpClientDispatcher"/> and a security 
    /// header value provider for authorization.
    /// </summary>
    /// <typeparam name="THttpClientDispatcherInterface">The interface type 
    /// that inherits from <see cref="IHttpClientDispatcher"/>.</typeparam>
    /// <typeparam name="THttpClientDispatcherImpl">The type that implements
    /// <typeparamref name="THttpClientDispatcherInterface"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="authenticationHeaderValue">The security header value provider.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher<THttpClientDispatcherInterface, THttpClientDispatcherImpl>(
        this IServiceCollection services,
        HttpClientAuthenticationHeaderValueProvider authenticationHeaderValue,
        Action<IServiceProvider, HttpClient> configureClient)
        where THttpClientDispatcherInterface : class, IHttpClientDispatcher
        where THttpClientDispatcherImpl : class, THttpClientDispatcherInterface
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .AddScoped<HttpClientAuthorizationHandler>()
            .AddScoped(_ => authenticationHeaderValue)
            .AddHttpClient<THttpClientDispatcherInterface, THttpClientDispatcherImpl>(configureClient)
            .ConfigurePrimaryHttpMessageHandler<HttpClientAuthorizationHandler>();
    }
}
