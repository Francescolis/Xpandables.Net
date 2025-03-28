
/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using Microsoft.Extensions.Options;

using Xpandables.Net.Http;

namespace Xpandables.Net.DependencyInjection;
/// <summary>
/// Provides extension methods for registering <see cref="IHttpSender"/> 
/// services in the service collection.
/// </summary>
public static class ServiceCollectionHttpExtensions
{
    /// <summary>
    /// Registers the <see cref="MapHttpOptions"/> configuration to 
    /// the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpRequestOptions(
        this IServiceCollection services)
        => services
            .AddTransient<IConfigureOptions<Http.MapHttpOptions>,
                MapHttpOptionsConfiguration>();

    /// <summary>
    /// Registers the specified <see cref="IHttpRequestFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <typeparam name="TRequestFactory">The type of the HTTP client 
    /// requests factory.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttRequestFactory<TRequestFactory>(
        this IServiceCollection services)
        where TRequestFactory : class, IHttpRequestFactory =>
        services.AddScoped<IHttpRequestFactory, TRequestFactory>();

    /// <summary>
    /// Registers the specified <see cref="IHttpResponseFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <typeparam name="TResponseFactory">The type of the HTTP client 
    /// responses factory.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpResponseFactory<TResponseFactory>(
        this IServiceCollection services)
        where TResponseFactory : class, IHttpResponseFactory =>
        services.AddScoped<IHttpResponseFactory, TResponseFactory>();

    /// <summary>
    /// Registers the default <see cref="HttpRequestFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpRequestFactory(
        this IServiceCollection services) =>
        services.AddXHttRequestFactory<HttpRequestFactory>();

    /// <summary>
    /// Registers the default <see cref="HttpResponseFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpResponseFactory(
        this IServiceCollection services) =>
        services.AddXHttpResponseFactory<HttpResponseFactory>();

    /// <summary>
    /// Registers the specified <typeparamref name="THttpSender"/> as
    /// <see cref="IHttpSender"/> implementation to the services.
    /// </summary>
    /// <typeparam name="THttpSender">The type of the HTTP client 
    /// sender.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder AddXHttpSender<THttpSender>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where THttpSender : class, IHttpSender =>
        services.AddHttpClient<IHttpSender, THttpSender>(configureClient);

    /// <summary>
    /// Registers the default <see cref="HttpSenderDefault"/> 
    /// implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder AddXHttpSender(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient) =>
        services
        .AddXHttpSender<HttpSenderDefault>(configureClient);
}
