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
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Rests;
using Xpandables.Net.Rests.Abstractions;
using Xpandables.Net.Rests.RequestBuilders;
using Xpandables.Net.Rests.ResponseBuilders;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring and registering services in an IServiceCollection.
/// </summary>
/// <remarks>This class contains static methods that extend the functionality of IServiceCollection, enabling
/// additional service registration patterns and configuration options commonly used in dependency injection
/// scenarios.</remarks>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IRestExtensions
{
    /// <summary>
    /// Provides extensions methods for <see cref="IServiceCollection"/>.
    /// </summary>
    extension(IServiceCollection services)
    {
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
        /// Adds the default implementation of the IRestRequestBuilder service to the dependency injection container.
        /// </summary>
        /// <remarks>Registers RestRequestBuilder with a scoped lifetime. Call this method during
        /// application startup to enable REST request building functionality via dependency injection.</remarks>
        /// <returns>The IServiceCollection instance with the IRestRequestBuilder service registered. This enables further
        /// configuration of the service collection.</returns>
        public IServiceCollection AddXRestRequestBuilder() =>
            services.AddXRestRequestBuilder<RestRequestBuilder>();

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
                .AddTransient(typeof(IRestRequestComposer<>), typeof(RestPatchComposer<>))
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
                .AddTransient(typeof(IRestResponseResultComposer<>), typeof(RestResponseResultComposer<>))
                .AddTransient(typeof(IRestResponseStreamComposer<>), typeof(RestResponseStreamComposer<>))
                .AddTransient<IRestResponseComposer, RestResponseNoContentComposer>();
    }
}
