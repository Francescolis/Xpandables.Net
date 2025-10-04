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
using System.Net.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.Rests;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Net.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for configuring and registering services in an IServiceCollection.
/// </summary>
/// <remarks>This class contains static methods that extend the functionality of IServiceCollection, enabling
/// additional service registration patterns and configuration options commonly used in dependency injection
/// scenarios.</remarks>
[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")]
public static class IServiceCollectionExtensions
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
    }
}
