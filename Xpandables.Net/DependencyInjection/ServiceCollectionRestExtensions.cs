
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
using Xpandables.Net.Http.Builders;
using Xpandables.Net.Http.Builders.Requests;
using Xpandables.Net.Http.Builders.Responses;

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
    /// Registers the default <see cref="RestRequestHandler{TRestRequest}"/> implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRestRequestHandler(
        this IServiceCollection services) =>
        services.AddScoped(typeof(IRestRequestHandler<>), typeof(RestRequestHandler<>));

    /// <summary>
    /// Registers various implementations of a request builder interface with the service collection.
    /// </summary>
    /// <param name="services">The collection of services to which the request builders are added for dependency injection.</param>
    /// <returns>The updated service collection with the new request builders registered.</returns>
    public static IServiceCollection AddXRestRequestBuilders(
        this IServiceCollection services) =>
        services
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestContextBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestPatchBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestQueryStringBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestCookieBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestHeaderBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestBasicAuthBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestByteArrayBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestFormUrlEncodedBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestMultipartBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestStreamBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestPatchBuilder<>))
            .AddScoped(typeof(IRestRequestBuilder<>), typeof(RestStringBuilder<>));

    /// <summary>
    /// Registers a scoped service for building REST responses within the service collection.
    /// </summary>
    /// <param name="services">The collection of services to which the REST response builder will be added.</param>
    /// <returns>The updated service collection with the new service registration.</returns>
    public static IServiceCollection AddXRestResponseBuilders(
        this IServiceCollection services) =>
        services
            .AddScoped<IRestResponseBuilder, RestResponseBuilder>();

    /// <summary>
    /// Registers the default <see cref="RestResponseHandler"/> implementation to the services.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>The <see cref="IServiceCollection"/> services.</returns>
    public static IServiceCollection AddXRestResponseHandler(
        this IServiceCollection services) =>
        services.AddScoped<IRestResponseHandler, RestResponseHandler>();

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
            .AddXRestRequestHandler()
            .AddXRestResponseHandler()
            .AddXRestClient<RestClient>(configureClient);
}
