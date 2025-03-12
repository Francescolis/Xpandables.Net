
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
/// Provides extension methods for registering <see cref="IRequestHttpSender"/> 
/// services in the service collection.
/// </summary>
public static class ServiceCollectionHttpExtensions
{
    /// <summary>
    /// Registers the <see cref="RequestOptions"/> configuration to 
    /// the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRequestOptions(
        this IServiceCollection services)
        => services
            .AddTransient<IConfigureOptions<RequestOptions>,
                RequestOptionsConfiguration>();

    /// <summary>
    /// Registers the specified <see cref="IRequestFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <typeparam name="TRequestFactory">The type of the HTTP client 
    /// requests factory.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRequestFactory<TRequestFactory>(
        this IServiceCollection services)
        where TRequestFactory : class, IRequestFactory =>
        services.AddScoped<IRequestFactory, TRequestFactory>();

    /// <summary>
    /// Registers the specified <see cref="IResponseFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <typeparam name="TResponseFactory">The type of the HTTP client 
    /// responses factory.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXResponseFactory<TResponseFactory>(
        this IServiceCollection services)
        where TResponseFactory : class, IResponseFactory =>
        services.AddScoped<IResponseFactory, TResponseFactory>();

    /// <summary>
    /// Registers the default <see cref="RequestFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRequestFactory(
        this IServiceCollection services) =>
        services.AddXRequestFactory<RequestFactory>();

    /// <summary>
    /// Registers the default <see cref="ResponseFactory"/> 
    /// implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXResponseFactory(
        this IServiceCollection services) =>
        services.AddXResponseFactory<ResponseFactory>();

    /// <summary>
    /// Registers the specified <typeparamref name="TRequestHttpSender"/> as
    /// <see cref="IRequestHttpSender"/> implementation to the services.
    /// </summary>
    /// <typeparam name="TRequestHttpSender">The type of the HTTP client 
    /// sender.</typeparam>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder AddXRequestHttpSender<TRequestHttpSender>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where TRequestHttpSender : class, IRequestHttpSender =>
        services.AddHttpClient<IRequestHttpSender, TRequestHttpSender>(configureClient);

    /// <summary>
    /// Registers the default <see cref="RequestHttpSenderDefault"/> 
    /// implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <param name="configureClient">The action to configure the HTTP client.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/>.</returns>
    public static IHttpClientBuilder AddXRequestHttpSender(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient) =>
        services
        .AddXRequestHttpSender<RequestHttpSenderDefault>(configureClient);
}
