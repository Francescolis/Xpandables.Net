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
/// Provides extension methods for configuring minimal API endpoint validation, result filtering, and execution result
/// header writing in ASP.NET Core applications.
/// </summary>
/// <remarks>Use the methods in this class to add middleware and service registrations that enable minimal API
/// features, such as automatic validation, consistent result filtering, and inclusion of execution result information
/// in HTTP response headers. These extensions are intended to be called during application startup when setting up the
/// application's request pipeline and dependency injection container.</remarks>
public static class IResultExtensions
{
    /// <summary>
    /// Adds Result MVC options configuration for Controller to the service collection.
    /// </summary>
    /// <remarks>Call this method during application startup to enable custom MVC options for
    /// XController. This method registers the necessary configuration as a singleton service.</remarks>
    /// <returns>The service collection with Controller MVC options configured. The same instance as the input is returned
    /// for chaining.</returns>
    public static IServiceCollection AddXControllerResultMvcOptions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IConfigureOptions<MvcOptions>, ControllerResulMvcOptions>();

        return services;
    }

    /// <summary>
    /// Adds the Result middleware to the application's service collection for dependency injection.
    /// </summary>
    /// <remarks>Registers ResultMiddleware as a singleton service. This method should be called during
    /// application startup to ensure the middleware is available for request processing.</remarks>
    /// <param name="services">The service collection to which the Result middleware will be added. Cannot be null.</param>
    /// <returns>The same IServiceCollection instance, enabling method chaining.</returns>
    public static IServiceCollection AddXResultMiddleware(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ResultMiddleware>();
        return services;
    }

    extension(IApplicationBuilder builder)
    {
        /// <summary>
        /// Adds the Result middleware to the application's request pipeline. This enables standardized result handling
        /// for HTTP responses.
        /// </summary>
        /// <remarks>This method should be called after all required services have been registered,
        /// typically in the application's startup configuration. The Result middleware provides consistent formatting
        /// and handling of API results across the application.</remarks>
        /// <returns>The <see cref="IApplicationBuilder"/> instance with the Result middleware configured. This allows for
        /// further chaining of middleware registrations.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Result middleware has not been registered in the application's services. Ensure that
        /// AddXResultSupport() is called during service registration before invoking this method.</exception>
        public IApplicationBuilder UseXResultMiddleware()
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (builder.ApplicationServices.GetService<ResultMiddleware>() is null)
                throw new InvalidOperationException(
                    "ResultMiddleware is not registered. " +
                    "Please ensure AddXResultMiddleware() is called during service registration.");

            builder.UseMiddleware<ResultMiddleware>();
            return builder;
        }
    }

    /// <summary>
    /// </summary>
    extension<TBuilder>(TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        /// <summary>
        /// Configures the builder to use the Result pattern API features, including filtering and validation.
        /// </summary>
        /// <returns>The builder instance configured with Result pattern API support. This enables both Result pattern filtering and
        /// validation.</returns>
        public TBuilder WithXResultSupport() =>
            builder
                .WithXResultFilter()
                .WithXResultValidation();

        /// <summary>
        /// Adds result pattern validation for endpoint results to the current builder.
        /// </summary>
        /// <remarks>This method configures the builder to use a minimal validation filter for endpoint
        /// results. Use this when only basic validation is required for endpoint responses. The returned builder can be
        /// further configured or used to build the endpoint.</remarks>
        /// <returns>The builder instance with minimal result pattern validation applied.</returns>
        public TBuilder WithXResultValidation()
        {
            ArgumentNullException.ThrowIfNull(builder);
            return builder.AddEndpointFilter(new ResultEndpointValidationFilter().InvokeAsync);
        }

        /// <summary>
        /// Adds a result pattern endpoint filter to the current builder configuration.
        /// </summary>
        /// <remarks>Use this method to ensure that endpoints only return results, which can help
        /// enforce consistent API responses. This method is typically used when configuring endpoints that should not
        /// include additional metadata or formatting.</remarks>
        /// <returns>The builder instance with the result pattern endpoint filter applied.</returns>
        public TBuilder WithXResultFilter() =>
            builder.AddEndpointFilter<TBuilder, ResultEndpointFilter>();
    }
}
