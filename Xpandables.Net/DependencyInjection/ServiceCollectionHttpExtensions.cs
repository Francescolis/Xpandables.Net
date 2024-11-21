
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
/// Provides extension methods for registering <see cref="IHttpClientSender"/> 
/// services in the service collection.
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
    /// Registers the specified <see cref="IHttpClientMessageFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <typeparam name="THttpClientMessageFactory">The type of the HTTP client 
    /// message factory.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientMessageFactory
        <THttpClientMessageFactory>(this IServiceCollection services)
        where THttpClientMessageFactory : class, IHttpClientMessageFactory =>
        services.AddScoped
            <IHttpClientMessageFactory, THttpClientMessageFactory>();

    /// <summary>
    /// Registers the default <see cref="HttpClientMessageFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXHttpClientMessageFactory(
        this IServiceCollection services) =>
        services.AddXHttpClientMessageFactory<HttpClientMessageFactory>();

    /// <summary>
    /// Registers the specified <see cref="IHttpClientSender"/> 
    /// implementation to the services.
    /// </summary>
    /// <typeparam name="THttpClientDispatcher">The type of the HTTP client 
    /// dispatcher.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder AddXHttpClientDispatcher<THttpClientDispatcher>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where THttpClientDispatcher : class, IHttpClientSender =>
        services.AddHttpClient
            <IHttpClientSender, THttpClientDispatcher>(configureClient);

    /// <summary>
    /// Registers the default <see cref="HttpClientSenderDefault"/> 
    /// implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder AddXHttpClientDispatcher(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient) =>
        services
        .AddXHttpClientDispatcher<HttpClientSenderDefault>(configureClient);
}
