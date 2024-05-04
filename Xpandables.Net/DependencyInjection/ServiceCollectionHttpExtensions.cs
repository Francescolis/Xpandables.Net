
/*******************************************************************************
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
********************************************************************************/
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Xpandables.Net.Http;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with methods to register Http services.
/// </summary>
public static class ServiceCollectionHttpExtensions
{
    /// <summary>
    /// Registers the <see cref="HttpClientOptions"/> configuration to 
    /// the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientOptions(
        this IServiceCollection services)
        => services
            .AddTransient<IConfigureOptions<HttpClientOptions>,
                HttpClientOptionsConfiguration>();

    /// <summary>
    /// Registers the <see cref="HttpClientOptions"/> builders to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientDispatcherBuilders(
        this IServiceCollection services)
        => services
            .AddScoped<IHttpClientResponseBuilder,
                SuccessHttpClientResponseBuilder>()
            .AddScoped<IHttpClientResponseBuilder,
                FailureHttpClientResponseBuilder>()
            .AddScoped(
                typeof(IHttpClientResponseResultBuilder<>),
                typeof(SuccessHttpClientResponseResultBuilder<>))
            .AddScoped(
                typeof(IHttpClientResponseResultBuilder<>),
                typeof(FailureHttpClientResponseResultBuilder<>))
            .AddScoped(
                typeof(IHttpClientResponseIAsyncResultBuilder<>),
                typeof(SuccessHttpClientResponseAsyncResultBuilder<>))
            .AddScoped(
                typeof(IHttpClientResponseIAsyncResultBuilder<>),
                typeof(FailureHttpClientResponseAsyncResultBuilder<>));


    /// <summary>
    /// Registers the <see cref="HttpClientDispatcherFactory"/> default 
    /// implementation to the services with scope life time.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/>
    /// is null.</exception>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientDispatcherFactory(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped
            <IHttpClientDispatcherFactory, HttpClientDispatcherFactory>();
        return services;
    }

    /// <summary>
    /// Registers the <typeparamref name="THttpClientDispatcherFactory"/> type 
    /// implementation to the services with scope life time.
    /// </summary>
    /// <typeparam name="THttpClientDispatcherFactory">The type to use to build 
    /// requests and responses.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientDispatcherFactory
        <THttpClientDispatcherFactory>(this IServiceCollection services)
        where THttpClientDispatcherFactory : class, IHttpClientDispatcherFactory
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped
            <IHttpClientDispatcherFactory, THttpClientDispatcherFactory>();
        return services;
    }

    /// <summary>
    ///  Registers the default implementation of 
    ///  <see cref="IHttpClientDispatcher"/> type and configures a 
    ///  binding with a named <see cref="HttpClient"/>. 
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">A delegate that is used to configure an 
    /// <see cref="HttpClient"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="configureClient"/> is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHttpClient
            <IHttpClientDispatcher, HttpClientDispatcherDefault>(configureClient);
    }

    /// <summary>
    ///  Registers the default implementation of 
    ///  <see cref="IHttpClientDispatcher"/> type and configures a 
    ///  binding with a named <see cref="HttpClient"/> and security header 
    ///  value provider for authorization. 
    /// </summary>
    /// <typeparam name="THttpClientAuthorizationHandler">The type that
    /// will be used to authorize the request.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">A delegate that is used to configure an 
    /// <see cref="HttpClient"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="configureClient"/> is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher
        <THttpClientAuthorizationHandler>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where THttpClientAuthorizationHandler : HttpClientAuthorizationHandler
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped<THttpClientAuthorizationHandler>()
            .AddHttpClient
            <IHttpClientDispatcher, HttpClientDispatcherDefault>(configureClient)
            .ConfigurePrimaryHttpMessageHandler<THttpClientAuthorizationHandler>();
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
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher
        <THttpClientDispatcherInterface, THttpClientDispatcherImpl>(
        this IServiceCollection services)
        where THttpClientDispatcherInterface : class, IHttpClientDispatcher
        where THttpClientDispatcherImpl : class, THttpClientDispatcherInterface
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHttpClient
            <THttpClientDispatcherInterface, THttpClientDispatcherImpl>();
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
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="services"/> is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher
        <THttpClientDispatcherInterface, THttpClientDispatcherImpl>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where THttpClientDispatcherInterface : class, IHttpClientDispatcher
        where THttpClientDispatcherImpl : class, THttpClientDispatcherInterface
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddHttpClient
            <THttpClientDispatcherInterface, THttpClientDispatcherImpl>(
            configureClient);
    }

    /// <summary>
    /// Registers the <typeparamref name="THttpClientDispatcherImpl"/> type as 
    /// <typeparamref name="THttpClientDispatcherInterface"/>
    /// to be used as <see cref="IHttpClientDispatcher"/> and a security header 
    /// value provider for authorization.
    /// </summary>
    /// <typeparam name="THttpClientDispatcherInterface">The interface type that 
    /// inherits from <see cref="IHttpClientDispatcher"/>.</typeparam>
    /// <typeparam name="THttpClientDispatcherImpl">The type that implements 
    /// <typeparamref name="THttpClientDispatcherInterface"/>.</typeparam>
    /// <typeparam name="THttpClientAuthorizationHandler">The type that will be
    /// used to authorize the request.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="services"/> 
    /// is null.</exception>
    public static IHttpClientBuilder AddXHttpClientDispatcher
        <THttpClientDispatcherInterface, THttpClientDispatcherImpl,
        THttpClientAuthorizationHandler>(
        this IServiceCollection services)
        where THttpClientDispatcherInterface : class, IHttpClientDispatcher
        where THttpClientDispatcherImpl : class, THttpClientDispatcherInterface
        where THttpClientAuthorizationHandler : HttpClientAuthorizationHandler
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .AddScoped<THttpClientAuthorizationHandler>()
            .AddHttpClient<THttpClientDispatcherInterface, THttpClientDispatcherImpl>()
            .ConfigurePrimaryHttpMessageHandler<THttpClientAuthorizationHandler>();
    }
}
