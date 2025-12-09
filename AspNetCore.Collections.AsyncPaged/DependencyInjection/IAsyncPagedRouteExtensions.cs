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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Provides extension methods for registering and configuring endpoint routes in an ASP.NET Core application.
/// </summary>
/// <remarks>These extension methods enable the discovery and registration of endpoint route implementations from
/// specified assemblies, as well as the configuration of the application's request pipeline to use those routes. Use
/// these methods to simplify the integration of modular endpoint routing patterns in your application.</remarks>
public static class IAsyncPagedRouteExtensions
{
    /// <summary>
    /// Adds AsyncPaged MVC options configuration for Controller to the service collection.
    /// </summary>
    /// <remarks>Call this method during application startup to enable custom MVC options for
    /// XController. This method registers the necessary configuration as a singleton service.</remarks>
    /// <returns>The service collection with Controller MVC options configured. The same instance as the input is returned
    /// for chaining.</returns>
    public static IServiceCollection AddXControllerAsyncPagedMvcOptions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IConfigureOptions<MvcOptions>, ControllerAsyncPagedMvcOptions>();

        return services;
    }

    /// <summary>
    /// </summary>
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        /// <summary>
        /// Adds an asynchronous paged endpoint filter to the builder configuration.
        /// </summary>
        /// <remarks>Use this method to enable support for paged asynchronous operations on endpoints.
        /// This is typically used when implementing APIs that return large datasets in pages. The filter must be
        /// compatible with the builder's endpoint configuration.</remarks>
        /// <returns>The builder instance with the asynchronous paged filter applied.</returns>
        public TBuilder WithXAsyncPagedFilter() =>
            builder.AddEndpointFilter<TBuilder, AsyncPagedEnpointFilter>();
    }
}
