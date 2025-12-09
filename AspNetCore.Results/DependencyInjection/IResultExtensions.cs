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
using System.Results;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
    /// Adds support for <see cref="Result"/> handling to the application's service collection.
    /// </summary>
    /// <param name="services">The service collection to act on. Cannot be null.</param>
    /// <param name="configure">An optional action to configure the <see cref="ResultSupportOptions"/></param>
    /// <returns>The same <paramref name="services"/> instance, with minimal result support registered.</returns>
    public static IServiceCollection AddXResultSupport(
        this IServiceCollection services,
        Action<ResultSupportOptions>? configure = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ResultSupportOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ResultMiddleware>();
        return services;
    }

    extension(IApplicationBuilder builder)
    {
        /// <summary>
        /// Adds minimal API endpoint validation and result filters to the application's request pipeline.
        /// </summary>
        /// <remarks>This extension method configures the middleware pipeline to apply validation and
        /// result filters specifically to minimal API endpoints. It should be called during application startup before
        /// mapping minimal API routes. The method does not affect non-minimal API endpoints.</remarks>
        /// <returns>The <see cref="IApplicationBuilder"/> instance with minimal API support filters applied.</returns>
        public IApplicationBuilder UseXResultSupport()
        {
            ArgumentNullException.ThrowIfNull(builder);

            if (builder.ApplicationServices.GetService<ResultMiddleware>() is null)
                throw new InvalidOperationException(
                    "ResultMiddleware is not registered. " +
                    "Please ensure AddXResultSupport() is called during service registration.");

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
