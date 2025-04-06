
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
/// Provides extension methods for registering <see cref="IRestClient"/> 
/// services in the service collection.
/// </summary>
public static class ServiceCollectionRestExtensions
{
    /// <summary>
    /// Registers the <see cref="RestOptions"/> configuration to 
    /// the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRestOptions(
        this IServiceCollection services)
        => services.AddTransient<IConfigureOptions<RestOptions>, RestOptionsConfiguration>();

    /// <summary>
    /// Registers the specified <see cref="IRestRequestFactory"/> implementation to the services.
    /// </summary>
    /// <typeparam name="TRestRequestFactory">The type of the HTTP client 
    /// requests factory.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRestRequestFactory<TRestRequestFactory>(
        this IServiceCollection services)
        where TRestRequestFactory : class, IRestRequestFactory =>
        services.AddScoped<IRestRequestFactory, TRestRequestFactory>();

    /// <summary>
    /// Registers the specified <see cref="IRestResponseFactory"/> implementation to the services.
    /// </summary>
    /// <typeparam name="TRestResponseFactory">The type of the HTTP client 
    /// responses factory.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRestResponseFactory<TRestResponseFactory>(
        this IServiceCollection services)
        where TRestResponseFactory : class, IRestResponseFactory =>
        services.AddScoped<IRestResponseFactory, TRestResponseFactory>();

    /// <summary>
    /// Registers the default <see cref="RestRequestFactory"/> implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRestRequestFactory(
        this IServiceCollection services) =>
        services.AddXRestRequestFactory<RestRequestFactory>();

    /// <summary>
    /// Registers the default <see cref="RestResponseFactory"/> implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRestResponseFactory(
        this IServiceCollection services) =>
        services.AddXRestResponseFactory<RestResponseFactory>();

    /// <summary>
    /// Registers the specified <typeparamref name="TRestClient"/> as
    /// <see cref="IRestClient"/> implementation to the services.
    /// </summary>
    /// <typeparam name="TRestClient">The type of the <see cref="IRestClient"/>.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder AddXRestClient<TRestClient>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where TRestClient : class, IRestClient =>
        services.AddHttpClient<IRestClient, TRestClient>(configureClient);

    /// <summary>
    /// Extends the service collection to add Rest client functionality. It configures the client using a provided
    /// action delegate.
    /// </summary>
    /// <param name="services">The collection of services to which the XRest client and related factories are added.</param>
    /// <param name="configureClient">An action that configures the HttpClient instance using the service provider.</param>
    /// <returns>Returns an IHttpClientBuilder for further configuration of the HTTP client.</returns>
    public static IHttpClientBuilder AddXRestClient(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient) =>
        services
            .AddXRestRequestFactory()
            .AddXRestResponseFactory()
            .AddXRestClient<RestClient>(configureClient);
}
